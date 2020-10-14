using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NetCoreRateLimit.Models;

namespace NetCoreRateLimit.Store
{
    public class MemoryCacheClientPolicyStore : MemoryCacheRateLimitStore<ClientRateLimitPolicy>, IClientPolicyStore
    {
        private readonly ClientRateLimitOptions _options;
        private readonly ClientRateLimitPolicies _policies;

        public MemoryCacheClientPolicyStore(
            IMemoryCache cache,
           ClientRateLimitOptions options = null,
            ClientRateLimitPolicies policies = null) : base(cache)
        {
            _options = options;
            _policies = policies;
        }

        public async Task SeedAsync()
        {
            // on startup, save the IP rules defined in appsettings
            if (_options != null && _policies?.ClientRules != null)
            {
                foreach (var rule in _policies.ClientRules)
                {
                    await SetAsync($"{_options.ClientPolicyPrefix}_{rule.ClientId}", new ClientRateLimitPolicy { ClientId = rule.ClientId, Rules = rule.Rules }).ConfigureAwait(false);
                }
            }
        }
    }
}