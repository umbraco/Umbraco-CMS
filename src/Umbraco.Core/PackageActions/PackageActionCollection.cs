using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PackageActions
{
    public sealed class PackageActionCollection : BuilderCollectionBase<IPackageAction>
    {
        public PackageActionCollection(IEnumerable<IPackageAction> items)
            : base(items)
        { }
    }
}
