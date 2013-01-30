using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Routing
{
    
	/// <summary>
	/// Provides context for the routing of a request.
	/// </summary>
    public class RoutingContext
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingContext"/> class.
		/// </summary>
		/// <param name="umbracoContext"> </param>
		/// <param name="contentFinders">The document lookups resolver.</param>
		/// <param name="contentLastChanceFinder"> </param>
		/// <param name="publishedContentStore">The content store.</param>
		/// <param name="niceUrlResolver">The nice urls resolver.</param>
		internal RoutingContext(
			UmbracoContext umbracoContext,
			IEnumerable<IContentFinder> contentFinders,
			IContentFinder contentLastChanceFinder,
            IPublishedContentStore publishedContentStore,
            NiceUrlProvider niceUrlResolver,
            IRoutesCache routesCache)
        {
			this.UmbracoContext = umbracoContext;
			this.PublishedContentFinders = contentFinders;
			this.PublishedContentLastChanceFinder = contentLastChanceFinder;
			this.PublishedContentStore = publishedContentStore;
        	this.NiceUrlProvider = niceUrlResolver;
            this.RoutesCache = routesCache;
        }

		/// <summary>
		/// Gets the Umbraco context.
		/// </summary>
		public UmbracoContext UmbracoContext { get; private set; }

		/// <summary>
		/// Gets the published content finders.
		/// </summary>
		internal IEnumerable<IContentFinder> PublishedContentFinders { get; private set; }

		/// <summary>
		/// Gets the published content last chance finder.
		/// </summary>
		internal IContentFinder PublishedContentLastChanceFinder { get; private set; }

		/// <summary>
		/// Gets the content store.
		/// </summary>
		internal IPublishedContentStore PublishedContentStore { get; private set; }

		/// <summary>
		/// Gets the nice urls provider.
		/// </summary>
		internal NiceUrlProvider NiceUrlProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="IRoutesCache"/>
        /// </summary>
        internal IRoutesCache RoutesCache { get; private set; }
    }
}