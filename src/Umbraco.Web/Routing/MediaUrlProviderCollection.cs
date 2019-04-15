using System.Collections.Generic;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class MediaUrlProviderCollection : BuilderCollectionBase<IMediaUrlProvider>
    {
        public MediaUrlProviderCollection(IEnumerable<IMediaUrlProvider> items)
            : base(items)
        {
        }
    }
}
