using BeetleX.FastHttpApi;
using System.Linq;
using System.Net;

namespace NetCoreRateLimit
{
    public class IpHeaderResolveContributor : IIpResolveContributor
    {

        private readonly string _headerName;

        public IpHeaderResolveContributor(
            string headerName)
        {
            _headerName = headerName;
        }

        public string ResolveIp(HttpRequest httpRequest)
        {
            IPAddress clientIp = null;


            if (!string.IsNullOrEmpty(httpRequest.Header[_headerName]))
            {
                clientIp = IpAddressUtil.ParseIp(httpRequest.Header[_headerName]);
            }

            return clientIp?.ToString();
        }
    }
}