using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NetCoreRateLimit.Models;

namespace NetCoreRateLimit
{
    public class ConfigHelper
    {
        private static RateLimitConfig rateLimitConfig;
        private const string CONFIG_FILE = "ratelimit.json";
        public static RateLimitConfig GetConfig(String file = CONFIG_FILE)
        {
            if (rateLimitConfig != null)
                return rateLimitConfig;

            if (System.IO.File.Exists(file))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(file, Encoding.UTF8))
                {
                    string configData = reader.ReadToEnd();
                    rateLimitConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<RateLimitConfig>(configData);

                    return rateLimitConfig;
                }
            }
            else
            {
                throw new FileNotFoundException($"Config file [{file}] not found!", file);
            }
        }
    }
}

