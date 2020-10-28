using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PackageActions
{
    [UmbracoVolatile]
    public class PackageActionCollectionBuilder : LazyCollectionBuilderBase<PackageActionCollectionBuilder, PackageActionCollection, IPackageAction>
    {
        protected override PackageActionCollectionBuilder This => this;
    }
}
