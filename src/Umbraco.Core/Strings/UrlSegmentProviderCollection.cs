using System.Collections.Generic;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderCollection : InjectCollectionBase<IUrlSegmentProvider>
    {
        public UrlSegmentProviderCollection(IEnumerable<IUrlSegmentProvider> items)
            : base(items)
        { }
    }
}
