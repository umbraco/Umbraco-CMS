using Umbraco.Core.Composing;

namespace Umbraco.Core.PackageActions
{
    internal class PackageActionCollectionBuilder : LazyCollectionBuilderBase<PackageActionCollectionBuilder, PackageActionCollection, IPackageAction>
    {
        protected override PackageActionCollectionBuilder This => this;
    }
}
