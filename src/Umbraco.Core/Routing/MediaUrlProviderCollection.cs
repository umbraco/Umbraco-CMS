using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing
{
    public class MediaUrlProviderCollection : BuilderCollectionBase<IMediaUrlProvider>
    {
        public MediaUrlProviderCollection(IEnumerable<IMediaUrlProvider> items)
            : base(items)
        { }
    }
}
