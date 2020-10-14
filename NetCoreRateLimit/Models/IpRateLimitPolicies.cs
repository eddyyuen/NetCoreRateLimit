using System.Collections.Generic;

namespace NetCoreRateLimit.Models
{
    public class IpRateLimitPolicies
    {
        public List<IpRateLimitPolicy> IpRules { get; set; } = new List<IpRateLimitPolicy>();
    }
}