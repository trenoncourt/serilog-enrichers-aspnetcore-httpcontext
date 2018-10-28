using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.AspnetcoreHttpcontext
{
    /// <summary>
    /// Extends <see cref="LoggerConfiguration"/> to add enrichers for <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
    /// capabilities.
    /// </summary>
    public static class LoggerEnrichmentConfigurationExtensions
    {
        /// <summary>
        /// Enrich log events with Aspnetcore httpContext properties.
        /// </summary>
        /// <param name="enrichmentConfiguration">Logger enrichment configuration.</param>
        /// <param name="serviceProvider"></param>
        /// <returns>Configuration object allowing method chaining.</returns>
        public static LoggerConfiguration WithAspnetcoreHttpcontext(this LoggerEnrichmentConfiguration enrichmentConfiguration, 
            IServiceProvider serviceProvider, 
            bool includeUserInfo, Action<IHttpContextAccessor, LogEvent, ILogEventPropertyFactory> customMethod = null)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));

            var enricher = serviceProvider.GetService<AspnetcoreHttpcontextEnricher>();
            if (includeUserInfo)
                enricher.IncludeUserInfo();

            if (customMethod != null)
                enricher.SetCustomAction(customMethod);

            return enrichmentConfiguration.With(enricher);
        }        
    }
}