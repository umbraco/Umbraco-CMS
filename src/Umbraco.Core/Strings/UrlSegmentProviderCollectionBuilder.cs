using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Strings;

public class UrlSegmentProviderCollectionBuilder : OrderedCollectionBuilderBase<UrlSegmentProviderCollectionBuilder, UrlSegmentProviderCollection, IUrlSegmentProvider>
{
    protected override UrlSegmentProviderCollectionBuilder This => this;
}
