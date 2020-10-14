using System.Collections.Generic;

namespace NetCoreRateLimit.Models
{
    public class ClientRateLimitPolicies
    {
        public List<ClientRateLimitPolicy> ClientRules { get; set; }
    }
}