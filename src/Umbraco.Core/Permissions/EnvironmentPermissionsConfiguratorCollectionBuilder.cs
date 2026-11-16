using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Permissions;

/// <summary>
/// Builds the <see cref="EnvironmentPermissionsConfiguratorCollection"/>.
/// </summary>
/// <remarks>
/// Removing a configurator is how a site overrides what a package decided.
/// </remarks>
public class EnvironmentPermissionsConfiguratorCollectionBuilder : SetCollectionBuilderBase<EnvironmentPermissionsConfiguratorCollectionBuilder, EnvironmentPermissionsConfiguratorCollection, IEnvironmentPermissionsConfigurator>
{
    /// <inheritdoc />
    protected override EnvironmentPermissionsConfiguratorCollectionBuilder This => this;
}
