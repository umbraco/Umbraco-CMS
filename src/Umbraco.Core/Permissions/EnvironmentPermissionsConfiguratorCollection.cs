using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Permissions;

/// <summary>
/// Represents a collection of <see cref="IEnvironmentPermissionsConfigurator"/> instances.
/// </summary>
public class EnvironmentPermissionsConfiguratorCollection : BuilderCollectionBase<IEnvironmentPermissionsConfigurator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentPermissionsConfiguratorCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the configurators.</param>
    public EnvironmentPermissionsConfiguratorCollection(Func<IEnumerable<IEnvironmentPermissionsConfigurator>> items)
        : base(items)
    {
    }
}
