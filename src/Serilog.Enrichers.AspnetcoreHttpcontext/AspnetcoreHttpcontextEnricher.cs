using System;
using System.IO;
using System.Linq;
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
        private Func<IHttpContextAccessor, object> _customAction = null;

        public AspnetcoreHttpcontextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _customAction = StandardEnricher;  // by default

        }        
        public void SetCustomAction(Func<IHttpContextAccessor, object> customAction)
        {
            _customAction = customAction;
        }        

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            HttpContext ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return;

            var httpContextCache = ctx.Items[$"serilog-enrichers-aspnetcore-httpcontext"];

            if (httpContextCache == null)
            {
                httpContextCache = _customAction.Invoke(_httpContextAccessor);
                ctx.Items[$"serilog-enrichers-aspnetcore-httpcontext"] = httpContextCache;
            }
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HttpContext", httpContextCache, true));             
        }

        public static object StandardEnricher(IHttpContextAccessor hca)
        {
            var ctx = hca.HttpContext;
            if (ctx == null) return null;

            var httpContextCache = new HttpContextCache
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
            return httpContextCache;
        }
    }
}
