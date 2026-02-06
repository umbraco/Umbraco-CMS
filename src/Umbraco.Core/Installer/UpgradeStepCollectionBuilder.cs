using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Installer;

/// <summary>
/// A builder for creating an <see cref="UpgradeStepCollection"/> with ordered <see cref="IUpgradeStep"/> instances.
/// </summary>
public class UpgradeStepCollectionBuilder : OrderedCollectionBuilderBase<UpgradeStepCollectionBuilder, UpgradeStepCollection, IUpgradeStep>
{
    /// <inheritdoc />
    protected override UpgradeStepCollectionBuilder This => this;

    /// <inheritdoc />
    protected override ServiceLifetime CollectionLifetime => ServiceLifetime.Scoped;
}
