using System.Collections.Generic;
using LightInject;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderCollectionBuilder : InjectCollectionBuilderBase<UrlSegmentProviderCollection, IUrlSegmentProvider>
    {
        public UrlSegmentProviderCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override UrlSegmentProviderCollection CreateCollection(IEnumerable<IUrlSegmentProvider> items)
        {
            return new UrlSegmentProviderCollection(items);
        }
    }
}
