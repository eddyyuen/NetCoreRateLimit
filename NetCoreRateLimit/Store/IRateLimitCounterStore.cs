namespace NetCoreRateLimit.Store
{
    using global::NetCoreRateLimit.Models;

    public interface IRateLimitCounterStore : IRateLimitStore<RateLimitCounter?>
    {
    }
}