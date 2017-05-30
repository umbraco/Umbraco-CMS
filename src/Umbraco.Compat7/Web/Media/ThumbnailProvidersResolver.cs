using System.Collections.Generic;
using Umbraco.Core.Media;
using Umbraco.Core.ObjectResolution;
using Umbraco.Web.Media.ThumbnailProviders;
using CoreCurrent = Umbraco.Core.Composing.Current;
using WebCurrent = Umbraco.Web.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Media
{
    public class ThumbnailProvidersResolver : WeightedObjectsResolverBase<ThumbnailProviderCollectionBuilder, ThumbnailProviderCollection, IThumbnailProvider>
    {
        private ThumbnailProvidersResolver(ThumbnailProviderCollectionBuilder builder)
            : base(builder)
        { }

        public static ThumbnailProvidersResolver Current { get; }
            = new ThumbnailProvidersResolver(CoreCurrent.Container.GetInstance<ThumbnailProviderCollectionBuilder>());

        public IEnumerable<IThumbnailProvider> Providers => WebCurrent.ThumbnailProviders;

        public string GetThumbnailUrl(string fileUrl) => WebCurrent.ThumbnailProviders.GetThumbnailUrl(fileUrl);
    }
}
