
    using Newtonsoft.Json.Linq;
    using BeetleX.EventArgs;
    using Serilog;
    using Serilog.Formatting.Compact;
using Bumblebee.Plugins;
using Bumblebee.Events;
using Bumblebee;
using System.Reflection;
using NetCoreRateLimit.Models;
using Serilog.Events;

namespace NetCoreRateLimit.Plugin
{
    public class Requesting_ClientID_RateLimit : IRequestingHandler, IPluginStatus
    {
        NetCoreRateLimit netCoreRateLimit;
        public string Name => "Requesting_ClientID_RateLimit";

        public string Description => "Requesting ClientID RateLimit";

        public PluginLevel Level => PluginLevel.High7;

        public bool Enabled { get; set; } = true;

        public void Execute(EventRequestingArgs e)
        {
            netCoreRateLimit.Invoke(e).GetAwaiter().GetResult();
        }

        private Gateway g;
        private ILogger log;
        private RateLimitConfig _config;
        public void Init(Gateway gateway, Assembly assembly)
        {
            _config = ConfigHelper.GetConfig();
            log = new LoggerConfiguration()
                //Error
                .WriteTo.Conditional(a => a.Level == LogEventLevel.Error, a => a.Async(a => a.File(new CompactJsonFormatter(),
                    "./logs/error_ratelimit_clientid__.clef", rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information),
                    bufferSize: 100))
                //Other
                .WriteTo.Conditional(a => (a.Level <= LogEventLevel.Warning), a => a.Async(a => a.File(new CompactJsonFormatter(), 
                    "./logs/log_ratelimit_clientid__.clef", rollingInterval: RollingInterval.Day,
                    restrictedToMinimumLevel: LogEventLevel.Information),
                    bufferSize: 100))
                         .CreateLogger();
            g = gateway;
            netCoreRateLimit = new NetCoreRateLimit(RateLimitType.CLIENT_ID);
            netCoreRateLimit.RequestBlocked += NetCoreRateLimit_RequestBlocked;
            netCoreRateLimit.Requested += NetCoreRateLimit_Requested;

            

        }

        private void NetCoreRateLimit_Requested(object sender, EventRequestedArgs e)
        {
            if (_config.WriteRequestedLog)
            {
                log.Information("Requested ClientId {@ClientID}  ClientIp {@ClientIp}, {@EventRequestedArgs}",
                                e.identity.ClientId, e.identity.ClientIp, e);
            }
        }

        private void NetCoreRateLimit_RequestBlocked(object sender, EventRequestBlockedArgs e)
        {
            if (_config.WriteRequestBlockedLog)
            {
                log.Warning("RequestBlocked ClientId {@ClientId} ClientIp {@ClientIp}, {@RequestBlockedArgs}",
                    e.identity.ClientId,
                    e.identity.ClientIp,
                    e);
            }
        }

        public void LoadSetting(JToken setting)
        {
            g.Pluginer.SetRequesting(Name);
        }

        public object SaveSetting()
        {

            return null;
        }
    }
}