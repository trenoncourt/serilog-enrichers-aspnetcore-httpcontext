using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.AspnetcoreHttpcontext
{
    public class AspnetcoreHttpcontextEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _includeUserInfo = false;
        private Action<IHttpContextAccessor, LogEvent, ILogEventPropertyFactory> _customAction = null;

        public AspnetcoreHttpcontextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;            
        }

        public void IncludeUserInfo()
        {
            _includeUserInfo = true;
        }

        public void SetCustomAction(Action<IHttpContextAccessor, LogEvent, ILogEventPropertyFactory> customAction)
        {
            _customAction = customAction;
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
                    ctx.Request.EnableRewind();
                
                    using (StreamReader reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, true, 1024, true))
                    {
                        httpContextCache.Body = reader.ReadToEnd();
                    }
                    
                    ctx.Request.Body.Position = 0;
                }
                ctx.Items[$"serilog-enrichers-aspnetcore-httpcontext"] = httpContextCache;
            }
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HttpContext", httpContextCache, true));

            if (_includeUserInfo)
            {
                var x = ctx.User as ClaimsPrincipal;
                if (x != null && x.Identity != null && x.Identity.IsAuthenticated)
                {
                    var claims = x.Claims.ToDictionary(c => c.Type, v => v.Value);
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserInfo", claims, false));
                }
            }

            if (_customAction != null)
            {
                _customAction.Invoke(_httpContextAccessor, logEvent, propertyFactory);
            }
                
        }
    }
}
