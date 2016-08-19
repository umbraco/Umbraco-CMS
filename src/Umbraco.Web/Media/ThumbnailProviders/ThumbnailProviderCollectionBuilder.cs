using LightInject;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Media;

namespace Umbraco.Web.Media.ThumbnailProviders
{
    public class ThumbnailProviderCollectionBuilder : WeightedCollectionBuilderBase<ThumbnailProviderCollectionBuilder, ThumbnailProviderCollection, IThumbnailProvider>
    {
        public ThumbnailProviderCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ThumbnailProviderCollectionBuilder This => this;
    }
}
