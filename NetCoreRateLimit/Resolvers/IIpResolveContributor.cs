using BeetleX.FastHttpApi;

namespace NetCoreRateLimit
{
    public interface IIpResolveContributor
    {
        string ResolveIp(HttpRequest httpRequest);
    }
}