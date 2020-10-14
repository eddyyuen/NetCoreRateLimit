namespace NetCoreRateLimit.Models
{
    public class ClientRateLimitPolicy : RateLimitPolicy
    {
        public string ClientId { get; set; }
    }
}