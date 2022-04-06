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
            builder.Services.AddTransient<IDetailedTelemetryProvider, DomainTelemetryProvider>();
            builder.Services.AddTransient<IDetailedTelemetryProvider, ExamineTelemetryProvider>();
            builder.Services.AddTransient<IDetailedTelemetryProvider, MacroTelemetryProvider>();
            builder.Services.AddTransient<IDetailedTelemetryProvider, NodeCountTelemetryProvider>();
            builder.Services.AddTransient<IDetailedTelemetryProvider, UserTelemetryProvider>();
            return builder;
        }
    }
}
