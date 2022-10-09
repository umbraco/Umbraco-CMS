using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.New.Cms.Core.Installer;

public class UpgradeStepCollectionBuilder : OrderedCollectionBuilderBase<UpgradeStepCollectionBuilder, UpgradeStepCollection, IUpgradeStep>
{
    protected override UpgradeStepCollectionBuilder This => this;

    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
