using System.Collections.Concurrent;
using Funnel.Server.Exceptions;

namespace Funnel.Server.Services;

public class ConnectionService(ILogger<ConnectionService> logger) : IConnectionService
{
    private readonly ConcurrentDictionary<string, FunnelService> _connections = new();

    public void Connect(string consumerName, FunnelService funnelService)
    {
        _connections.TryAdd(consumerName, funnelService);
    }

    public void DisConnect(string consumerName)
    {
        _connections.Remove(consumerName, out _);
    }

    public async Task<FunneledResponse> SendAndWaitForResponse(string consumerName, FunneledRequest request)
    {
        if (_connections.TryGetValue(consumerName, out var funnelService))
        {
            await funnelService.SendRequest(request);
            try
            {
                var response = await funnelService.WaitForResponse(request.Id);
                return response;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while waiting for response");
                throw;
            }
        }

        logger.LogWarning("Consumer {ConsumerName} not found", consumerName);
        throw new ConsumerNotFoundException($"Consumer {consumerName} not found");
    }
}