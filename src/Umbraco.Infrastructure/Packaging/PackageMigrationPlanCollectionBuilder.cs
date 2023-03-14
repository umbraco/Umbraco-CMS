using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Packaging;

public class PackageMigrationPlanCollectionBuilder : LazyCollectionBuilderBase<PackageMigrationPlanCollectionBuilder,
    PackageMigrationPlanCollection, PackageMigrationPlan>
{
    protected override PackageMigrationPlanCollectionBuilder This => this;
}
