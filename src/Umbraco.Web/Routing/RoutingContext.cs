using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Routing
{
    
	/// <summary>
	/// Provides context for the routing of a request.
	/// </summary>
    internal class RoutingContext
    {
		/// <summary>
		/// Initializes a new instance of the <see cref="RoutingContext"/> class.
		/// </summary>
		/// <param name="umbracoContext">The Umbraco context.</param>
		/// <param name="documentLookupsResolver">The document lookups resolver.</param>
		/// <param name="contentStore">The content store.</param>
		/// <param name="niceUrlResolver">The nice urls resolver.</param>
        public RoutingContext(
			UmbracoContext umbracoContext,
			DocumentLookupsResolver documentLookupsResolver,
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
		public DocumentLookupsResolver DocumentLookupsResolver { get; private set; }

		/// <summary>
		/// Gets the content store.
		/// </summary>
        public ContentStore ContentStore { get; private set; }

		/// <summary>
		/// Gets the nice urls provider.
		/// </summary>
		public NiceUrlProvider NiceUrlProvider { get; private set; }
    }
}