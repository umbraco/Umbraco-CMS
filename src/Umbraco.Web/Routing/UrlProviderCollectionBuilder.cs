using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class UrlProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlProviderCollectionBuilder, UrlProviderCollection, IUrlProvider>
    {
        public UrlProviderCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override UrlProviderCollectionBuilder This => this;
    }
}
