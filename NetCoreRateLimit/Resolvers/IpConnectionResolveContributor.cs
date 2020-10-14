
using BeetleX.FastHttpApi;

namespace NetCoreRateLimit
{
    public class IpConnectionResolveContributor : IIpResolveContributor
    {
      

        public IpConnectionResolveContributor()
        {
        }

        public string ResolveIp(HttpRequest httpRequest)
        {
            return httpRequest.RemoteIPAddress?.ToString();
        }
    }
}