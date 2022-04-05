using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry.Providers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DependencyInjection
{
    public static class UmbracoBuilder_TelemetryProviders
    {
        public static IUmbracoBuilder AddTelemetryProviders(this IUmbracoBuilder builder)
        {
            builder.Services.AddTransient<IDetailedTelemetryProvider, ContentTelemetryProvider>();
            return builder;
        }
    }
}
