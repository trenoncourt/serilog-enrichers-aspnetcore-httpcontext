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
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IpAddress", ctx.Connection.RemoteIpAddress.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Host", ctx.Request.Host));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Path", ctx.Request.Path));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Method", ctx.Request.Method));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Querystring", ctx.Request.QueryString));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Headers", ctx.Request.Headers.ToDictionary(x => x.Key, y => y.Value)));
            ctx.Request.EnableRewind();
            string bodyString = null;
            if (ctx.Request.Method != "GET")
            {
                using (StreamReader reader = new StreamReader(ctx.Request.Body, Encoding.UTF8))
                {
                    bodyString = reader.ReadToEnd();
                } 
            }

            if (!string.IsNullOrEmpty(bodyString))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Body", bodyString));
            }
        }
    }
}