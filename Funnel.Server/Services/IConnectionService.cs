using System.Threading.Channels;

namespace Funnel.Server.Services;

public interface IConnectionService
{
    void Connect(string consumerName, FunnelService funnelService);
    void DisConnect(string consumerName);
    
    Task<FunneledResponse> SendAndWaitForResponse(string consumerName, FunneledRequest request);
}