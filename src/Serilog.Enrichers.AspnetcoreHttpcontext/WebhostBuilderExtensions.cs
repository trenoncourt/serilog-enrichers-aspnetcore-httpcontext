using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.AspNetCore;
using Serilog.Core;

namespace Serilog.Enrichers.AspnetcoreHttpcontext
{
    public static class WebhostBuilderExtensions
    {
        /// <summary>Sets Serilog as the logging provider.</summary>
        /// <remarks>
        /// A <see cref="T:Microsoft.AspNetCore.Hosting.WebHostBuilderContext" /> is supplied so that configuration and hosting information can be used.
        /// The logger will be shut down when application services are disposed.
        /// </remarks>
        /// <param name="builder">The web host builder to configure.</param>
        /// <param name="configureLogger">The delegate for configuring the <see cref="T:Serilog.LoggerConfiguration" /> that will be used to construct a <see cref="T:Microsoft.Extensions.Logging.Logger" />.</param>
        /// <param name="preserveStaticLogger">Indicates whether to preserve the value of <see cref="P:Serilog.Log.Logger" />.</param>
        /// <returns>The web host builder.</returns>
        public static IWebHostBuilder UseSerilog(this IWebHostBuilder builder,
            Action<IServiceProvider, WebHostBuilderContext, LoggerConfiguration> configureLogger,
            bool preserveStaticLogger = false)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configureLogger == null)
                throw new ArgumentNullException(nameof(configureLogger));

            builder.ConfigureServices((context, collection) =>
            {
                collection.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                var provider = collection.BuildServiceProvider();
                var hca = provider.GetRequiredService<IHttpContextAccessor>();

                collection.AddSingleton<AspnetcoreHttpcontextEnricher>(); 

                LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
                configureLogger(collection.BuildServiceProvider(), context, loggerConfiguration);
                
                Logger logger = loggerConfiguration.CreateLogger();
                if (preserveStaticLogger)
                {
                    collection.AddSingleton(services => (ILoggerFactory)new SerilogLoggerFactory(logger, true));
                }
                else
                {
                    Log.Logger = logger;
                    collection.AddSingleton(services => (ILoggerFactory)new SerilogLoggerFactory(null, true));
                }
            });
            return builder;
        }        
    }
}