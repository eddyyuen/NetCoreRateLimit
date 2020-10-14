using Microsoft.Extensions.Caching.Memory;
using NetCoreRateLimit.Middleware;
using NetCoreRateLimit.Models;
using NetCoreRateLimit.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bumblebee;
using BeetleX.FastHttpApi;
using Bumblebee.Events;
using Serilog;
using Serilog.Formatting.Compact;

namespace NetCoreRateLimit
{
    public class NetCoreRateLimit
    {
        private readonly IRateLimitProcessor _processor;
        private readonly RateLimitOptions _options;
        private readonly RateLimitConfig _config;
        private readonly IMemoryCache _cache;

        private MemoryCacheIpPolicyStore memoryCacheIpPolicyStore;
        private MemoryCacheClientPolicyStore memoryCacheClientPolicyStore;
        private MemoryCacheRateLimitCounterStore memoryCacheRateLimitCounterStore;
        private RateLimitConfiguration rateLimitConfiguration;

        //public delegate void RequestBlockedDelegate(ClientRequestIdentity identity, RateLimitCounter rateLimitCounter, RateLimitRule rateLimitRule);

        //public delegate void RequestingDelegate(ClientRequestIdentity identity, Dictionary<RateLimitRule, RateLimitCounter> rules);
        //public RequestBlockedDelegate RequestBlocked;
        //public RequestingDelegate Requested;
        public event EventHandler<EventRequestBlockedArgs> RequestBlocked;
        public event EventHandler<EventRequestedArgs> Requested;

        

        public NetCoreRateLimit(RateLimitType rateLimitType)
        {          
            _config = ConfigHelper.GetConfig();

            _cache = new MemoryCache(new MemoryCacheOptions());

            memoryCacheIpPolicyStore = new MemoryCacheIpPolicyStore(_cache, _config.IpRateLimitOptions, _config.IpRateLimitPolicies);
            memoryCacheClientPolicyStore = new MemoryCacheClientPolicyStore(_cache, _config.ClientRateLimitOptions, _config.ClientRateLimitPolicies);
            memoryCacheRateLimitCounterStore = new MemoryCacheRateLimitCounterStore(_cache);

            rateLimitConfiguration = new RateLimitConfiguration(_config.IpRateLimitOptions, _config.ClientRateLimitOptions);

            switch (rateLimitType)
            {
                case RateLimitType.CLIENT_ID:
                    _processor = new ClientRateLimitProcessor(_config.ClientRateLimitOptions
                        , memoryCacheRateLimitCounterStore, memoryCacheClientPolicyStore, rateLimitConfiguration);
                    _options = _config.ClientRateLimitOptions as RateLimitOptions;
                    break;
                case RateLimitType.IP:
                    _processor = new IpRateLimitProcessor(_config.IpRateLimitOptions
                       , memoryCacheRateLimitCounterStore, memoryCacheIpPolicyStore, rateLimitConfiguration);
                    _options = _config.IpRateLimitOptions as RateLimitOptions;
                    break;
                default:
                    _processor = new IpRateLimitProcessor(_config.IpRateLimitOptions
                       , memoryCacheRateLimitCounterStore, memoryCacheIpPolicyStore, rateLimitConfiguration);
                    _options = _config.IpRateLimitOptions as RateLimitOptions;
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="clientId">客户端ID，如有值，覆盖其他数据</param>
        /// <param name="clientIp">客户端IP，如有值，覆盖其他数据</param>
        /// <returns></returns>
        public async Task Invoke(EventRequestingArgs e, string clientId =null,string clientIp=null)
        {

            HttpRequest httpRequest = e.Request;
            // compute identity from request
            var identity = ResolveIdentity(httpRequest);
            if (!String.IsNullOrEmpty(clientId))
            {
                identity.ClientId = clientId;
            }
            if (!String.IsNullOrEmpty(clientIp))
            {
                identity.ClientIp = clientIp;
            }

            // check white list
            if (_processor.IsWhitelisted(identity))
            {
                return;
            }


            var rules = await _processor.GetMatchingRulesAsync(identity);

            var rulesDict = new Dictionary<RateLimitRule, RateLimitCounter>();

            foreach (var rule in rules)
            {
                // increment counter
                var rateLimitCounter = await _processor.ProcessRequestAsync(identity, rule);

                if (rule.Limit > 0)
                {
                    // check if key expired
                    if (rateLimitCounter.Timestamp + rule.PeriodTimespan.Value < DateTime.UtcNow)
                    {
                        continue;
                    }

                    // check if limit is reached
                    if (rateLimitCounter.Count > rule.Limit)
                    {
                        //compute retry after value
                        var retryAfter = rateLimitCounter.Timestamp.RetryAfterFrom(rule);

                        //// log blocked request
                        //LogBlockedRequest(context, identity, rateLimitCounter, rule);

                        //if (_options.RequestBlockedBehavior != null)
                        //{
                        //    await _options.RequestBlockedBehavior(context, identity, rateLimitCounter, rule);
                        //}
                       // RequestBlocked?.Invoke(identity, rateLimitCounter, rule);
                        if (RequestBlocked != null)
                        {
                            RequestBlocked.Invoke(this, new EventRequestBlockedArgs() { identity = identity, rateLimitCounter = rateLimitCounter, rateLimitRule = rule });
                        }
                        //// break execution
                        await ReturnQuotaExceededResponse(e, rule, retryAfter);


                        return;
                    }
                }
                // if limit is zero or less, block the request.
                else
                {
                    //// log blocked request
                    //LogBlockedRequest(context, identity, rateLimitCounter, rule);

                    //if (_options.RequestBlockedBehavior != null)
                    //{
                    //    await _options.RequestBlockedBehavior(context, identity, rateLimitCounter, rule);
                    //}
                    if(RequestBlocked != null)
                    {
                        RequestBlocked.Invoke(this, new EventRequestBlockedArgs() { identity = identity, rateLimitCounter = rateLimitCounter, rateLimitRule = rule });
                    }

                   

                    //// break execution (Int32 max used to represent infinity)
                    await ReturnQuotaExceededResponse(e, rule, int.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));

                    return;
                }

                rulesDict.Add(rule, rateLimitCounter);
            }

            // set X-Rate-Limit headers for the longest period
            if (rulesDict.Any() && !_options.DisableRateLimitHeaders)
            {
                var rule = rulesDict.OrderByDescending(x => x.Key.PeriodTimespan).FirstOrDefault();
                var headers = _processor.GetRateLimitHeaders(rule.Value, rule.Key);

                //  headers.Context = context;

                // context.Response.OnStarting(SetRateLimitHeaders, state: headers);
                await SetRateLimitHeaders(e, headers);
            }

            if (rulesDict.Any())
            {
                //Requested?.Invoke(identity, rulesDict);
                if (Requested != null)
                {
                    Requested.Invoke(this, new EventRequestedArgs() { identity = identity, rules = rulesDict});
                }
            }

            await Task.CompletedTask;
        }

        public virtual ClientRequestIdentity ResolveIdentity(HttpRequest httpRequest)
        {
            string clientIp = null;
            string clientId = null;

            if (rateLimitConfiguration.ClientResolvers?.Any() == true)
            {
                foreach (var resolver in rateLimitConfiguration.ClientResolvers)
                {
                    clientId = resolver.ResolveClient(httpRequest);

                    if (!string.IsNullOrEmpty(clientId))
                    {
                        break;
                    }
                }
            }

            if (rateLimitConfiguration.IpResolvers?.Any() == true)
            {
                foreach (var resolver in rateLimitConfiguration.IpResolvers)
                {
                    clientIp = resolver.ResolveIp(httpRequest);

                    if (!string.IsNullOrEmpty(clientIp))
                    {
                        break;
                    }
                }
            }

            return new ClientRequestIdentity
            {
                ClientIp = clientIp,
                Path = httpRequest.Path.ToString().ToLowerInvariant(),
                HttpVerb = httpRequest.Method.ToLowerInvariant(),
                ClientId = clientId ?? "anon"
            };
        }
        private Task SetRateLimitHeaders(EventRequestingArgs e, object rateLimitHeaders)
        {
            var headers = (RateLimitHeaders)rateLimitHeaders;

            e.Response.Header["X-Rate-Limit-Limit"] = headers.Limit;
            e.Response.Header["X-Rate-Limit-Remaining"] = headers.Remaining;
            e.Response.Header["X-Rate-Limit-Reset"] = headers.Reset;

            return Task.CompletedTask;
        }

        public virtual Task ReturnQuotaExceededResponse(EventRequestingArgs e, RateLimitRule rule, string retryAfter)
        {
            var message = string.Format(
                _options.QuotaExceededResponse?.Content ??
                _options.QuotaExceededMessage ??
                "API calls quota exceeded! maximum admitted {0} per {1}.", rule.Limit, rule.Period, retryAfter);

            if (!_options.DisableRateLimitHeaders)
            {
                e.Response.Header["Retry-After"] = retryAfter;
            }
            e.Response.SetStatus(_options.QuotaExceededResponse?.StatusCode.ToString() ?? _options.HttpStatusCode.ToString(), message);
           
            e.Response.Result(new JsonResult(new { returnCode = _options.QuotaExceededResponse?.StatusCode ?? _options.HttpStatusCode, message }));

            e.Cancel = true;
            return Task.CompletedTask;
        }
    }

    public enum RateLimitType
    {
        CLIENT_ID,
        IP
    }

    public class EventRequestBlockedArgs
    {
        public EventRequestBlockedArgs() { }
        public ClientRequestIdentity identity { get; set; }
        public RateLimitCounter rateLimitCounter { get; set; }
        public RateLimitRule rateLimitRule { get; set; }
    }

    public class EventRequestedArgs
    {
        public EventRequestedArgs() { }
        public ClientRequestIdentity identity { get; set; }
        public Dictionary<RateLimitRule, RateLimitCounter> rules { get; set; }
     
    }
}
