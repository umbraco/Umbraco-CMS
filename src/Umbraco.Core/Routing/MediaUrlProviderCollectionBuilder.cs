using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

public class MediaUrlProviderCollectionBuilder : OrderedCollectionBuilderBase<MediaUrlProviderCollectionBuilder, MediaUrlProviderCollection, IMediaUrlProvider>
{
    protected override MediaUrlProviderCollectionBuilder This => this;
}
