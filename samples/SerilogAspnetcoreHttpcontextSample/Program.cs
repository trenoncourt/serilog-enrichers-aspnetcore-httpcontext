using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace SerilogAspnetcoreHttpcontextSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((provider, context, loggerConfiguration) =>
                {
                    loggerConfiguration.MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.WithAspnetcoreHttpcontext(provider, 
                            includeUserInfo: true,
                            customMethod: CustomEnricherLogic)
                        .WriteTo.Console(
                            outputTemplate:
                            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{HttpContext} {NewLine}{Exception}")
                        .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                        {
                            IndexFormat = "serilog-enrichers-aspnetcore-httpcontext-{0:yyyy.MM}"
                        });
                });

        private static void CustomEnricherLogic(IHttpContextAccessor ctx, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            HttpContext context = ctx.HttpContext;
            if (context == null)
            {
                return;
            }
            var userInfo = context.Items[$"serilog-enrichers-aspnetcore-userinfo"] as UserInfo;
            if (userInfo == null)
            {
                var user = context.User.Identity;
                if (user == null || !user.IsAuthenticated) return;
                userInfo = new UserInfo
                {
                    Name = user.Name,
                    Claims = context.User.Claims.ToDictionary(x => x.Type, y => y.Value)
                };
                context.Items[$"serilog-enrichers-aspnetcore-userinfo"] = userInfo;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserInfo", userInfo, true));
        }

        public class UserInfo
        {
            public string Name { get; set; }
            public Dictionary<string, string> Claims { get; set; }
        }
    }
}