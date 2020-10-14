using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetCoreRateLimit.Models;

namespace NetCoreRateLimit.Store
{
    public class MemoryCacheIpPolicyStore : MemoryCacheRateLimitStore<IpRateLimitPolicies>, IIpPolicyStore
    {
        private readonly IpRateLimitOptions _options;
        private readonly IpRateLimitPolicies _policies;

        public MemoryCacheIpPolicyStore(
            IMemoryCache cache,
            IpRateLimitOptions options = null,
            IpRateLimitPolicies policies = null) : base(cache)
        {
            _options = options;
            _policies = policies;
        }

        public async Task SeedAsync()
        {
            // on startup, save the IP rules defined in appsettings
            if (_options != null && _policies != null)
            {
                await SetAsync($"{_options.IpPolicyPrefix}", _policies).ConfigureAwait(false);
            }
        }
    }
}