using System.Text;
using System.Web;
using Funnel.Server;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Net.Client;

namespace Funnel.CliClient;

public class Worker(
    ILogger<Worker> logger,
    Uri serverUrl,
    Uri localUrl,
    string consumerId,
    IServiceScopeFactory serviceScopeFactory,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = GrpcChannel.ForAddress(serverUrl);
        var client = new FunnelServer.FunnelServerClient(channel);

        // using var connection = client.Connect(new ConnectRequest { ConsumerName = "Funnel.CliClient" },
        //     cancellationToken: stoppingToken);
        // await foreach (var message in connection.ResponseStream.ReadAllAsync(cancellationToken: stoppingToken))
        // {
        //     logger.LogInformation("Received message: {Message}", message);
        // }
        var funnel = client.Funnel([new Metadata.Entry("ConsumerId", consumerId)],
            cancellationToken: stoppingToken);
        await foreach (var request in funnel.ResponseStream.ReadAllAsync(stoppingToken))
        {
            logger.LogInformation("Received request: {Request}", request);
            using var scope = serviceScopeFactory.CreateScope();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();

            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var (key, value) in request.Query)
            {
                query.Add(key, value);
            }

            var uri = new UriBuilder(localUrl)
            {
                Path = request.Path,
                Query = query.ToString(),
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Parse(request.Method), uri.ToString());

            var headers = request.Headers.Clone();
            headers.Remove("Content-Type", out var contentType);
            headers.Remove("Content-Length");

            foreach (var (key, value) in headers)
            {
                httpRequest.Headers.Add(key, value);
            }

            if (request.HasBody)
            {
                httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, contentType);
            }

            var response = await httpClient.SendAsync(httpRequest, stoppingToken);
            var responseMessage = new FunneledResponse()
            {
                Id = request.Id,
                Status = (int)response.StatusCode,
                Body = await response.Content.ReadAsStringAsync(stoppingToken),
                ClientId = consumerId
            };
            responseMessage.Headers.Add(response.Headers.ToDictionary(x => x.Key, x => x.Value.ToString()));

            await funnel.RequestStream.WriteAsync(responseMessage, stoppingToken);
        }


        logger.LogInformation("Connection closed");

        hostApplicationLifetime.StopApplication();
    }
}