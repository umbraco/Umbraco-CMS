using System.Collections.Generic;
using Umbraco.Web.PublishedCache;

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
	    /// <param name="urlProvider">The nice urls provider.</param>
	    internal RoutingContext(
			UmbracoContext umbracoContext,
			IEnumerable<IContentFinder> contentFinders,
			IContentFinder contentLastChanceFinder,
            UrlProvider urlProvider)
        {
			UmbracoContext = umbracoContext;
			PublishedContentFinders = contentFinders;
			PublishedContentLastChanceFinder = contentLastChanceFinder;
        	UrlProvider = urlProvider;
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
		/// Gets the urls provider.
		/// </summary>
		public UrlProvider UrlProvider { get; private set; }
    }
}