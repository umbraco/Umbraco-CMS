using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Installer;

/// <summary>
/// Represents a collection of <see cref="IInstallStep"/> instances used during new Umbraco installations.
/// </summary>
public class NewInstallStepCollection : BuilderCollectionBase<IInstallStep>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewInstallStepCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that returns the collection of install steps.</param>
    public NewInstallStepCollection(Func<IEnumerable<IInstallStep>> items)
        : base(items)
    {
    }
}
