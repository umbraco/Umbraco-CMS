using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Strings
{
    public class UrlSegmentProviderResolver : ManyObjectsResolverBase<UrlSegmentProviderCollectionBuilder, UrlSegmentProviderCollection, IUrlSegmentProvider>
    {
        private UrlSegmentProviderResolver(UrlSegmentProviderCollectionBuilder builder)
            : base(builder)
        { }

        public static UrlSegmentProviderResolver Current { get; }
            = new UrlSegmentProviderResolver(CoreCurrent.Container.GetInstance<UrlSegmentProviderCollectionBuilder>());

        public IEnumerable<IUrlSegmentProvider> Providers => CoreCurrent.UrlSegmentProviders;
    }
}
