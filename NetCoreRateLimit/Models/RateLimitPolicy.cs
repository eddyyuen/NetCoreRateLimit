using System.Collections.Generic;

namespace NetCoreRateLimit.Models
{
    public class RateLimitPolicy
    {
        public List<RateLimitRule> Rules { get; set; } = new List<RateLimitRule>();
    }
}
