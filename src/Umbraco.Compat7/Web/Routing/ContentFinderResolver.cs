using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.DI.Current;
using WebCurrent = Umbraco.Web.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Routing
{
    public class ContentFinderResolver : ManyObjectsResolverBase<ContentFinderCollectionBuilder, ContentFinderCollection, IContentFinder>
    {
        private ContentFinderResolver(ContentFinderCollectionBuilder builder) 
            : base(builder)
        { }

        public static ContentFinderResolver Current { get; }
            = new ContentFinderResolver(CoreCurrent.Container.GetInstance<ContentFinderCollectionBuilder>());

        public IEnumerable<IContentFinder> Finders => WebCurrent.ContentFinders;
    }
}
