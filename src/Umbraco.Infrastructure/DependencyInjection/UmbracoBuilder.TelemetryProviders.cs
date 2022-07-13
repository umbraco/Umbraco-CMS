using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

public static class UmbracoBuilder_TelemetryProviders
{
    public static IUmbracoBuilder AddTelemetryProviders(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IDetailedTelemetryProvider, ContentTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, DomainTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, ExamineTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, LanguagesTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, MacroTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, MediaTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, NodeCountTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, PropertyEditorTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, UserTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, SystemInformationTelemetryProvider>();
        return builder;
    }
}
