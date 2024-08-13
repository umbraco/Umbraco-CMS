using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Installer;

public class UpgradeStepCollectionBuilder : OrderedCollectionBuilderBase<UpgradeStepCollectionBuilder, UpgradeStepCollection, IUpgradeStep>
{
    protected override UpgradeStepCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
