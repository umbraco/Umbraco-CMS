using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     A collection of <see cref="PackageMigrationPlan" />
/// </summary>
public class PackageMigrationPlanCollection : BuilderCollectionBase<PackageMigrationPlan>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackageMigrationPlanCollection"/> class.
    /// </summary>
    /// <param name="items">A delegate that returns an <see cref="IEnumerable{PackageMigrationPlan}"/> representing the collection of package migration plans.</param>
    public PackageMigrationPlanCollection(Func<IEnumerable<PackageMigrationPlan>> items)
        : base(items)
    {
    }
}
