using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PackageActions
{
    public class PackageActionCollectionBuilder : LazyCollectionBuilderBase<PackageActionCollectionBuilder, PackageActionCollection, IPackageAction>
    {
        protected override PackageActionCollectionBuilder This => this;
    }
}
