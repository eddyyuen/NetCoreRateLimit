﻿using System;
using System.Linq;
using System.Net;


namespace NetCoreRateLimit
{
//    public class ReversProxyIpParser : RemoteIpParser
//    {
//        private readonly string _realIpHeader;

//        public ReversProxyIpParser(string realIpHeader)
//        {
//            _realIpHeader = realIpHeader;
//        }

//        public override IPAddress GetClientIp(HttpContext context)
//        {
//            if (context.Request.Headers.Keys.Contains(_realIpHeader, StringComparer.CurrentCultureIgnoreCase))
//            {
//                return ParseIp(context.Request.Headers[_realIpHeader].Last());
//            }

//            return base.GetClientIp(context);
//        }

//        public virtual IPAddress GetClientIp(HttpContext context)
//        {
//            return context.Connection.RemoteIpAddress;
//        }
//    }
}