using NetCoreRateLimit.Models;
using System.Threading.Tasks;

namespace NetCoreRateLimit.Store
{
    public interface IIpPolicyStore : IRateLimitStore<IpRateLimitPolicies>
    {
        Task SeedAsync();
    }
}