using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

/// <summary>
/// Registers the <see cref="ConvertLocalLink"/> migration with the Umbraco composer pipeline.
/// </summary>
[Obsolete("Scheduled for removal in Umbraco 18.")]
public class ConvertLocalLinkComposer : IComposer
{
    /// <summary>
    /// Registers the services required for processing and converting local links as part of the Umbraco 15.0.0 upgrade migration.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> used to register migration-related services.</param>
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ITypedLocalLinkProcessor, LocalLinkBlockListProcessor>();
        builder.Services.AddSingleton<ITypedLocalLinkProcessor, LocalLinkBlockGridProcessor>();
        builder.Services.AddSingleton<ITypedLocalLinkProcessor, LocalLinkRteProcessor>();
        builder.Services.AddSingleton<LocalLinkProcessor>();
        builder.Services.AddSingleton<LocalLinkProcessorForFaultyLinks>();
        builder.Services.AddSingleton<LocalLinkMigrationTracker>();
    }
}
