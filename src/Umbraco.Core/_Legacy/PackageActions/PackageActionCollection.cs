using System.Collections.Generic;
using Umbraco.Core.DI;

namespace Umbraco.Core._Legacy.PackageActions
{
    internal class PackageActionCollection : BuilderCollectionBase<IPackageAction>
    {
        public PackageActionCollection(IEnumerable<IPackageAction> items) 
            : base(items)
        { }
    }
}
