using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing
{
    public class UrlProviderCollection : BuilderCollectionBase<IUrlProvider>
    {
        public UrlProviderCollection(IEnumerable<IUrlProvider> items)
            : base(items)
        { }
    }
}
