using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Installer;

/// <summary>
/// Represents a collection of <see cref="IUpgradeStep"/> instances used during Umbraco upgrades.
/// </summary>
public class UpgradeStepCollection : BuilderCollectionBase<IUpgradeStep>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpgradeStepCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of upgrade steps.</param>
    public UpgradeStepCollection(Func<IEnumerable<IUpgradeStep>> items)
        : base(items)
    {
    }
}
