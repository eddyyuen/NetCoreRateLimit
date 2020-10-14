using NetCoreRateLimit.Models;

namespace NetCoreRateLimit
{
    public interface ICounterKeyBuilder
    {
        string Build(ClientRequestIdentity requestIdentity, RateLimitRule rule);
    }
}