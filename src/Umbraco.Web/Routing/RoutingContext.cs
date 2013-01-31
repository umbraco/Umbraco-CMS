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
		/// <param name="urlProvider">The nice urls provider.</param>
		internal RoutingContext(
			UmbracoContext umbracoContext,
			IEnumerable<IContentFinder> contentFinders,
			IContentFinder contentLastChanceFinder,
            IPublishedContentStore publishedContentStore,
            UrlProvider urlProvider,
            IRoutesCache routesCache)
        {
			this.UmbracoContext = umbracoContext;
			this.PublishedContentFinders = contentFinders;
			this.PublishedContentLastChanceFinder = contentLastChanceFinder;
			this.PublishedContentStore = publishedContentStore;
        	this.UrlProvider = urlProvider;
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
		/// Gets the urls provider.
		/// </summary>
		internal UrlProvider UrlProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="IRoutesCache"/>
        /// </summary>
        internal IRoutesCache RoutesCache { get; private set; }
    }
}