using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class UrlProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlProviderCollectionBuilder, UrlProviderCollection, IUrlProvider>
    {
        protected override UrlProviderCollectionBuilder This => this;
    }
}
