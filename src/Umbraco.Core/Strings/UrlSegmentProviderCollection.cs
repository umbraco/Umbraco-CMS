using System.Collections.Generic;
using Umbraco.Core.DI;

namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderCollection : BuilderCollectionBase<IUrlSegmentProvider>
    {
        public UrlSegmentProviderCollection(IEnumerable<IUrlSegmentProvider> items)
            : base(items)
        { }
    }
}
