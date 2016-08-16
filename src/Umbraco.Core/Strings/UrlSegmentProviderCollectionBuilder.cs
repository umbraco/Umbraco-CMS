using System.Collections.Generic;
using LightInject;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlSegmentProviderCollectionBuilder, UrlSegmentProviderCollection, IUrlSegmentProvider>
    {
        public UrlSegmentProviderCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override UrlSegmentProviderCollectionBuilder This => this;

        protected override UrlSegmentProviderCollection CreateCollection(IEnumerable<IUrlSegmentProvider> items)
        {
            return new UrlSegmentProviderCollection(items);
        }
    }
}
