using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

public class UrlProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlProviderCollectionBuilder, UrlProviderCollection, IUrlProvider>
{
    protected override UrlProviderCollectionBuilder This => this;
}
