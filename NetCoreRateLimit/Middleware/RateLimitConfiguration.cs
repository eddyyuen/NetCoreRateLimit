using Microsoft.Extensions.Options;
using NetCoreRateLimit.Models;
using System;
using System.Collections.Generic;

namespace NetCoreRateLimit.Middleware
{
    public class RateLimitConfiguration : IRateLimitConfiguration
    {
        public IList<IClientResolveContributor> ClientResolvers { get; } = new List<IClientResolveContributor>();
        public IList<IIpResolveContributor> IpResolvers { get; } = new List<IIpResolveContributor>();

        public virtual ICounterKeyBuilder EndpointCounterKeyBuilder { get; } = new PathCounterKeyBuilder();

        public virtual Func<double> RateIncrementer { get; } = () => 1;

        public RateLimitConfiguration(
            IpRateLimitOptions ipOptions,
            ClientRateLimitOptions clientOptions)
        {
            IpRateLimitOptions = ipOptions;
            ClientRateLimitOptions = clientOptions;

            RegisterResolvers();
        }

        protected readonly IpRateLimitOptions IpRateLimitOptions;
        protected readonly ClientRateLimitOptions ClientRateLimitOptions;

        protected virtual void RegisterResolvers()
        {
            if (!string.IsNullOrEmpty(ClientRateLimitOptions?.ClientIdHeader))
            {
                ClientResolvers.Add(new ClientHeaderResolveContributor(ClientRateLimitOptions.ClientIdHeader));
            }

            // the contributors are resolved in the order of their collection index
            if (!string.IsNullOrEmpty(IpRateLimitOptions?.RealIpHeader))
            {
                IpResolvers.Add(new IpHeaderResolveContributor( IpRateLimitOptions.RealIpHeader));
            }

            IpResolvers.Add(new IpConnectionResolveContributor());
        }
    }
}
