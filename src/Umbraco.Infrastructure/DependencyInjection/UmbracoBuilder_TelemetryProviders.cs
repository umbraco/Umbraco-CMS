using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;

namespace Umbraco.Cms.Infrastructure.DependencyInjection;

/// <summary>
/// Contains extension methods for registering telemetry providers with the Umbraco builder.
/// </summary>
public static class UmbracoBuilder_TelemetryProviders
{
    /// <summary>
    /// Registers all default <see cref="IDetailedTelemetryProvider"/> implementations with the Umbraco builder's service collection.
    /// This enables telemetry data collection from various sources such as content, domains, media, users, and more.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> to which telemetry providers will be added.</param>
    /// <returns>The same <see cref="IUmbracoBuilder"/> instance so that multiple calls can be chained.</returns>
    public static IUmbracoBuilder AddTelemetryProviders(this IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IDetailedTelemetryProvider, ContentTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, DomainTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, ExamineTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, LanguagesTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, MediaTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, NodeCountTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, PropertyEditorTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, UserTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, SystemTroubleshootingInformationTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, DeliveryApiTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, WebhookTelemetryProvider>();
        builder.Services.AddTransient<IDetailedTelemetryProvider, BlocksInRichTextTelemetryProvider>();
        return builder;
    }
}
