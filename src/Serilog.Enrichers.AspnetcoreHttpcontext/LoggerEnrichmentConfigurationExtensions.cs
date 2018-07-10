using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Configuration;

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
        public static LoggerConfiguration WithAspnetcoreHttpcontext(this LoggerEnrichmentConfiguration enrichmentConfiguration, IServiceProvider serviceProvider)
        {
            if (enrichmentConfiguration == null) throw new ArgumentNullException(nameof(enrichmentConfiguration));
            return enrichmentConfiguration.With(serviceProvider.GetService<AspnetcoreHttpcontextEnricher>());
        }
    }
}