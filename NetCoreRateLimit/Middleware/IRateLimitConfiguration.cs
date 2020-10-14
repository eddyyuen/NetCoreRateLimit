using System;
using System.Collections.Generic;

namespace NetCoreRateLimit.Middleware
{
    public interface IRateLimitConfiguration
    {
   

        ICounterKeyBuilder EndpointCounterKeyBuilder { get; }

        Func<double> RateIncrementer { get; }
    }
}