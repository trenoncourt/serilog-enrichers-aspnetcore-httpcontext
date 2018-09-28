using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.AspnetcoreHttpcontext
{
    public class AspnetcoreHttpcontextEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspnetcoreHttpcontextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            HttpContext ctx = _httpContextAccessor.HttpContext;
            if (ctx == null)
            {
                return;
            }
            var httpContextCache = ctx.Items[$"serilog-enrichers-aspnetcore-httpcontext"] as HttpContextCache;
            if (httpContextCache == null)
            {
                httpContextCache = new HttpContextCache
                {
                    IpAddress = ctx.Connection.RemoteIpAddress.ToString(),
                    Host = ctx.Request.Host.ToString(),
                    Path = ctx.Request.Path.ToString(),
                    IsHttps = ctx.Request.IsHttps,
                    Scheme = ctx.Request.Scheme,
                    Method = ctx.Request.Method,
                    ContentType = ctx.Request.ContentType,
                    Protocol = ctx.Request.Protocol,
                    QueryString = ctx.Request.QueryString.ToString(),
                    Query = ctx.Request.Query.ToDictionary(x => x.Key, y => y.Value.ToString()),
                    Headers = ctx.Request.Headers.ToDictionary(x => x.Key, y => y.Value.ToString()),
                    Cookies = ctx.Request.Cookies.ToDictionary(x => x.Key, y => y.Value.ToString())
                };

                if (ctx.Request.ContentLength.HasValue && ctx.Request.ContentLength > 0)
                {
                    using (StreamReader reader = new StreamReader(ctx.Request.Body, Encoding.UTF8))
                    {
                        httpContextCache.Body = reader.ReadToEnd();
                    }
                }
                ctx.Items[$"serilog-enrichers-aspnetcore-httpcontext"] = httpContextCache;
            }
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HttpContext", httpContextCache, true));

        }
    }
}
