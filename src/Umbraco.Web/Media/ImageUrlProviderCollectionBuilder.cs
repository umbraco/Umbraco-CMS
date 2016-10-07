using LightInject;
using Umbraco.Core.DI;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media
{
    internal class ImageUrlProviderCollectionBuilder : OrderedCollectionBuilderBase<ImageUrlProviderCollectionBuilder, ImageUrlProviderCollection, IImageUrlProvider>
    {
        public ImageUrlProviderCollectionBuilder(IServiceContainer container) : base(container)
        {}

        protected override ImageUrlProviderCollectionBuilder This => this;
    }
}
