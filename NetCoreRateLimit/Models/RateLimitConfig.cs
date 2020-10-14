using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NetCoreRateLimit.Models
{
    public class RateLimitConfig
    {
        public bool WriteRequestedLog { get; set; }
        public bool WriteRequestBlockedLog { get; set; }

        [JsonProperty(PropertyName = "IpRateLimiting")]
        public IpRateLimitOptions IpRateLimitOptions { get; set; }
        
        public IpRateLimitPolicies IpRateLimitPolicies { get; set; }
        [JsonProperty(PropertyName = "ClientRateLimiting")] 
        public ClientRateLimitOptions ClientRateLimitOptions { get; set; }
        public ClientRateLimitPolicies ClientRateLimitPolicies { get; set; }
    }
}
