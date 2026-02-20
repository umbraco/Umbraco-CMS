using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
internal class MigrateSingleBlockListComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ITypedSingleBlockListProcessor, SingleBlockListBlockListProcessor>();
        builder.Services.AddSingleton<ITypedSingleBlockListProcessor, SingleBlockListBlockGridProcessor>();
        builder.Services.AddSingleton<ITypedSingleBlockListProcessor, SingleBlockListRteProcessor>();
        builder.Services.AddSingleton<SingleBlockListProcessor>();
        builder.Services.AddSingleton<SingleBlockListConfigurationCache>();
    }
}
