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
		/// <param name="umbracoContext">The Umbraco context.</param>
		/// <param name="documentLookupsResolver">The document lookups resolver.</param>
		/// <param name="contentStore">The content store.</param>
		/// <param name="niceUrlResolver">The nice urls resolver.</param>
        internal RoutingContext(
			UmbracoContext umbracoContext,
			DocumentLookupsResolver2 documentLookupsResolver,
            ContentStore contentStore,
			NiceUrlProvider niceUrlResolver)
        {
        	this.UmbracoContext = umbracoContext;
			this.DocumentLookupsResolver = documentLookupsResolver;
            this.ContentStore = contentStore;
        	this.NiceUrlProvider = niceUrlResolver;
        }

		/// <summary>
		/// Gets the Umbraco context.
		/// </summary>
		public UmbracoContext UmbracoContext { get; private set; }

		/// <summary>
		/// Gets the document lookups resolver.
		/// </summary>
		internal DocumentLookupsResolver2 DocumentLookupsResolver { get; private set; }

		/// <summary>
		/// Gets the content store.
		/// </summary>
		internal ContentStore ContentStore { get; private set; }

		/// <summary>
		/// Gets the nice urls provider.
		/// </summary>
		internal NiceUrlProvider NiceUrlProvider { get; private set; }
    }
}