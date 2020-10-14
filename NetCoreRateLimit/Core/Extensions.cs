﻿using NetCoreRateLimit.Models;
using System;
using System.Text.RegularExpressions;

namespace NetCoreRateLimit
{
    public static class Extensions
    {
        public static bool IsUrlMatch(this string source, string value, bool useRegex)
        {
            if (useRegex)
            {
                return IsRegexMatch(source, value);
            }
            return source.IsWildCardMatch(value);
        }

        public static bool IsWildCardMatch(this string source, string value)
        {
            return source != null && value != null && source.ToLowerInvariant().IsMatch(value.ToLowerInvariant());
        }

        public static bool IsRegexMatch(this string source, string value)
        {
            return source != null && value != null && Regex.IsMatch(source, value, RegexOptions.IgnoreCase);
        }

        public static string RetryAfterFrom(this DateTime timestamp, RateLimitRule rule)
        {
            var diff = timestamp + rule.PeriodTimespan.Value - DateTime.UtcNow;
            var seconds = Math.Max(diff.TotalSeconds, 1);

            return $"{seconds:F0}";
        }

        public static TimeSpan ToTimeSpan(this string timeSpan)
        {
            var l = timeSpan.Length - 1;
            var value = timeSpan.Substring(0, l);
            var type = timeSpan.Substring(l, 1);

            switch (type)
            {
                case "d": return TimeSpan.FromDays(double.Parse(value));
                case "h": return TimeSpan.FromHours(double.Parse(value));
                case "m": return TimeSpan.FromMinutes(double.Parse(value));
                case "s": return TimeSpan.FromSeconds(double.Parse(value));
                default: throw new FormatException($"{timeSpan} can't be converted to TimeSpan, unknown type {type}");
            }
        }
    }
}