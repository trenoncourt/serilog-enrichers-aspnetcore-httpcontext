using System;
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
                            customMethod: CustomEnricherLogic)
                        .WriteTo.Console(
                            outputTemplate:
                            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{HttpContext} {NewLine}{Exception}");
                    //.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                    //{
                    //    IndexFormat = "serilog-enrichers-aspnetcore-httpcontext-{0:yyyy.MM}"
                    //});
                });

        private static MyObject CustomEnricherLogic(IHttpContextAccessor ctx)
        {
            var context = ctx.HttpContext;
            if (context == null) return null;
            
            var myInfo = new MyObject
            {
                Path = context.Request.Path.ToString(),
                Host = context.Request.Host.ToString(),
                Method = context.Request.Method
            };
            return myInfo;
        }

        public class MyObject
        {
            public string Path { get; set; }
            public string Host { get; set; }
            public string Method { get; set; }
        }
    }
}