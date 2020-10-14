using System.Threading.Tasks;
using NetCoreRateLimit.Models;
namespace NetCoreRateLimit.Store
{
    public interface IClientPolicyStore : IRateLimitStore<ClientRateLimitPolicy>
    {
        Task SeedAsync();
    }
}