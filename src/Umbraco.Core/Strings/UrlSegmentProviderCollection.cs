using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderCollection : BuilderCollectionBase<IUrlSegmentProvider>
    {
        public UrlSegmentProviderCollection(IEnumerable<IUrlSegmentProvider> items)
            : base(items)
        { }
    }
}
