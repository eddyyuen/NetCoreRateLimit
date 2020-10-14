using System.Collections.Generic;
using System.Net;

namespace NetCoreRateLimit
{
    public interface IIpAddressParser
    {
        bool ContainsIp(string ipRule, string clientIp);

        bool ContainsIp(List<string> ipRules, string clientIp);

        bool ContainsIp(List<string> ipRules, string clientIp, out string rule);

       // IPAddress GetClientIp(HttpContext context);

        IPAddress ParseIp(string ipAddress);
    }
}