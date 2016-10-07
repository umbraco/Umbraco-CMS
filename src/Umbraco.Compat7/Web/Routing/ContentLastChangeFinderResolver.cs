using System;
using Umbraco.Core.DI;
using CoreCurrent = Umbraco.Core.DI.Current;
using WebCurrent = Umbraco.Web.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Routing
{
    public class ContentLastChangeFinderResolver
    {
        private ContentLastChangeFinderResolver()
        { }

        public static ContentLastChangeFinderResolver Current { get; } = new ContentLastChangeFinderResolver();

        public IContentFinder Finder => WebCurrent.LastChanceContentFinder;

        public void SetFinder(IContentFinder finder)
        {
            if (finder == null) throw new ArgumentNullException(nameof(finder));
            var lastChance = finder as IContentLastChanceFinder ?? new FinderWrapper(finder);
            CoreCurrent.Container.RegisterSingleton(_ => lastChance);
        }

        private class FinderWrapper : IContentLastChanceFinder
        {
            private readonly IContentFinder _inner;

            public FinderWrapper(IContentFinder inner)
            {
                _inner = inner;
            }

            public bool TryFindContent(PublishedContentRequest contentRequest)
            {
                return _inner.TryFindContent(contentRequest);
            }
        }
    }
}
