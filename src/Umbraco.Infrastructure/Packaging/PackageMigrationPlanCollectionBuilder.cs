using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
/// Provides functionality to build and manage a collection of package migration plans.
/// </summary>
public class PackageMigrationPlanCollectionBuilder : WeightedCollectionBuilderBase<PackageMigrationPlanCollectionBuilder,
    PackageMigrationPlanCollection, PackageMigrationPlan>
{
    protected override PackageMigrationPlanCollectionBuilder This => this;
}
