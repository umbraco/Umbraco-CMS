using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class UrlProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlProviderCollectionBuilder, UrlProviderCollection, IUrlProvider>
    {
        public UrlProviderCollectionBuilder(IContainer container)
            : base(container)
        { }

        protected override UrlProviderCollectionBuilder This => this;
    }
}
