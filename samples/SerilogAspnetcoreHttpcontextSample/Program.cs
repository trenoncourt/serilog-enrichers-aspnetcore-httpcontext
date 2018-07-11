using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace SerilogAspnetcoreHttpcontextSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
//            Log.Logger = new LoggerConfiguration()
//                .MinimumLevel.Debug()
//                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
//                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {IpAddress}{Host}{Path}{Querystring} {Method}{NewLine}{Headers}{NewLine}{Body}{NewLine}{Exception}")
//                .CreateLogger();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((provider, context, loggerConfiguration) =>
                {
                    loggerConfiguration.MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                        .Enrich.WithAspnetcoreHttpcontext(provider)
                        .WriteTo.Console(
                            outputTemplate:
                            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{HttpContext} {NewLine}{Exception}");
                });
    }
}