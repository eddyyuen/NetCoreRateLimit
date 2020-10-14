using BeetleX.FastHttpApi;

namespace NetCoreRateLimit
{
    public interface IClientResolveContributor
    {
        string ResolveClient(HttpRequest httpRequest);
      
    }
}
