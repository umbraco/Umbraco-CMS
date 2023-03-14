using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     A collection of <see cref="PackageMigrationPlan" />
/// </summary>
public class PackageMigrationPlanCollection : BuilderCollectionBase<PackageMigrationPlan>
{
    public PackageMigrationPlanCollection(Func<IEnumerable<PackageMigrationPlan>> items)
        : base(items)
    {
    }
}
