using LightInject;
using Umbraco.Core.DI;

namespace Umbraco.Core._Legacy.PackageActions
{
    internal class PackageActionCollectionBuilder : LazyCollectionBuilderBase<PackageActionCollectionBuilder, PackageActionCollection, IPackageAction>
    {
        public PackageActionCollectionBuilder(IServiceContainer container) 
            : base(container)
        { }

        protected override PackageActionCollectionBuilder This => this;
    }
}
