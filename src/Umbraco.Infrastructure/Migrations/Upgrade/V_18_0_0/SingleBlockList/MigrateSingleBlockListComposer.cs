using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Only used by the obsolete MigrateSingleBlockList, which is no longer part of the upgrade plan. Scheduled for removal in Umbraco 20.")]
internal class MigrateSingleBlockListComposer : IComposer
{
    /// <summary>
    /// Registers the services required to support the migration of the single block list feature in Umbraco 18.0.0.
    /// This includes processors for block list, block grid, and rich text editor (RTE) single block lists, as well as supporting configuration and caching services.
    /// </summary>
    /// <param name="builder">The <see cref="IUmbracoBuilder"/> used to register migration-related services.</param>
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ITypedSingleBlockListProcessor, SingleBlockListBlockListProcessor>();
        builder.Services.AddSingleton<ITypedSingleBlockListProcessor, SingleBlockListBlockGridProcessor>();
        builder.Services.AddSingleton<ITypedSingleBlockListProcessor, SingleBlockListRteProcessor>();
        builder.Services.AddSingleton<SingleBlockListProcessor>();
        builder.Services.AddSingleton<SingleBlockListConfigurationCache>();
    }
}
