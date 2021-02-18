using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Strings
{
    public class UrlSegmentProviderCollection : BuilderCollectionBase<IUrlSegmentProvider>
    {
        public UrlSegmentProviderCollection(IEnumerable<IUrlSegmentProvider> items)
            : base(items)
        { }
    }
}
