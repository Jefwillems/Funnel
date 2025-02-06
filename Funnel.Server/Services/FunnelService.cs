using System.Collections.Concurrent;
using System.Threading.Channels;
using Grpc.Core;

namespace Funnel.Server.Services;

public class FunnelService(
    ILogger<FunnelService> logger,
    IHostApplicationLifetime hostApplicationLifetime,
    IConnectionService connectionService)
    : FunnelServer.FunnelServerBase
{
    // channel which handles messages from http to grpc (client)
    private readonly Channel<FunneledRequest> _requestChannel = Channel.CreateUnbounded<FunneledRequest>();

    // channel which handles messages from grpc (client) to http
    private readonly Channel<string> _responsesUpdatedChannel = Channel.CreateUnbounded<string>();

    // stores responses from the client 
    private readonly ConcurrentDictionary<string, FunneledResponse> _responses = new();

    public override async Task Funnel(IAsyncStreamReader<FunneledResponse> requestStream,
        IServerStreamWriter<FunneledRequest> responseStream, ServerCallContext context)
    {
        var consumerId = context.RequestHeaders.GetValue("ConsumerId");
        if (consumerId == null)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ConsumerId header is required"));
        }

        connectionService.Connect(consumerId, this);
        logger.LogInformation("Connected consumer {ConsumerId}", consumerId);

        var cancellationToken =
            CancellationTokenSource.CreateLinkedTokenSource(hostApplicationLifetime.ApplicationStopping,
                context.CancellationToken);

        var requestLoop = Task.Run(async () =>
            {
                try
                {
                    await foreach (var message in _requestChannel.Reader.ReadAllAsync(cancellationToken.Token))
                    {
                        logger.LogInformation("Sending grpc message: {Message}", message);
                        await responseStream.WriteAsync(message, cancellationToken.Token);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while funneling messages to the client");
                }
            },
            cancellationToken.Token);

        var responseLoop = Task.Run(async () =>
            {
                try
                {
                    await foreach
                        (var message in requestStream.ReadAllAsync(cancellationToken: cancellationToken.Token))
                    {
                        logger.LogInformation("Received grpc message: {Message}", message);
                        _responses.TryAdd(message.Id, message);
                        await _responsesUpdatedChannel.Writer.WriteAsync(message.Id, cancellationToken.Token);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error while reading responses from the client");
                }
            },
            cancellationToken.Token);

        await Task.WhenAll(requestLoop, responseLoop);
    }

    public async Task SendRequest(FunneledRequest request)
    {
        await _requestChannel.Writer.WriteAsync(request);
    }

    public async Task<FunneledResponse> WaitForResponse(string requestId)
    {
        while (await _responsesUpdatedChannel.Reader.WaitToReadAsync())
        {
            if (_responses.TryRemove(requestId, out var response))
            {
                return response;
            }
        }

        // when does this happen?
        throw new Exception("Response not found");
    }
}