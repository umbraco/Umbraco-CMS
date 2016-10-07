using System.Collections.Generic;
using Umbraco.Core.DI;

namespace Umbraco.Web.Routing
{
    public class UrlProviderCollection : BuilderCollectionBase<IUrlProvider>
    {
        public UrlProviderCollection(IEnumerable<IUrlProvider> items) 
            : base(items)
        { }
    }
}
