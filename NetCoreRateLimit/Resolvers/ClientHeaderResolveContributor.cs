
using BeetleX.FastHttpApi;
using System.Linq;

namespace NetCoreRateLimit
{
    public class ClientHeaderResolveContributor : IClientResolveContributor
    {
        private readonly string _headerName;

        public ClientHeaderResolveContributor(string headerName)
        {
            _headerName = headerName;
        }
        public string ResolveClient(HttpRequest httpRequest)
        {
            string clientId = httpRequest.Header[_headerName];

            if (!string.IsNullOrEmpty(clientId))
            {
                return clientId;
            }

            return null;
        }
    }
}