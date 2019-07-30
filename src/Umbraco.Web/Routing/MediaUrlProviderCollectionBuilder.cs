using Umbraco.Core.Composing;

namespace Umbraco.Web.Routing
{
    public class MediaUrlProviderCollectionBuilder : OrderedCollectionBuilderBase<MediaUrlProviderCollectionBuilder, MediaUrlProviderCollection, IMediaUrlProvider>
    {
        protected override MediaUrlProviderCollectionBuilder This => this;
    }
}
