using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core._Legacy.PackageActions
{
    public sealed class PackageActionCollection : BuilderCollectionBase<IPackageAction>
    {
        public PackageActionCollection(IEnumerable<IPackageAction> items)
            : base(items)
        { }
    }
}
