using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using WebCurrent = Umbraco.Web.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Routing
{
    public class UrlProviderResolver : ManyObjectsResolverBase<UrlProviderCollectionBuilder, UrlProviderCollection, IUrlProvider>
    {
        private UrlProviderResolver(UrlProviderCollectionBuilder builder) 
            : base(builder)
        { }

        public static UrlProviderResolver Current { get; }
            = new UrlProviderResolver(CoreCurrent.Container.GetInstance<UrlProviderCollectionBuilder>());

        public IEnumerable<IUrlProvider> Providers => WebCurrent.UrlProviders;
    }
}
