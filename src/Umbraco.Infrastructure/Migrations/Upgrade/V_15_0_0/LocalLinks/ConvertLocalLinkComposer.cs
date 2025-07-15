using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class ConvertLocalLinkComposer : IComposer
{
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
