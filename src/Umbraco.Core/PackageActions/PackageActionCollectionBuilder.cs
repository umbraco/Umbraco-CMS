using Umbraco.Core.Composing;

namespace Umbraco.Core.PackageActions
{
    public class PackageActionCollectionBuilder : LazyCollectionBuilderBase<PackageActionCollectionBuilder, PackageActionCollection, IPackageAction>
    {
        protected override PackageActionCollectionBuilder This => this;
    }
}
