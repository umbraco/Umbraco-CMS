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
		/// <param name="documentLookups">The document lookups resolver.</param>
		/// <param name="documentLastChanceLookup"> </param>
		/// <param name="contentStore">The content store.</param>
		/// <param name="niceUrlResolver">The nice urls resolver.</param>
		internal RoutingContext(
			UmbracoContext umbracoContext,
			IEnumerable<IDocumentLookup> documentLookups,
			IDocumentLastChanceLookup documentLastChanceLookup,
            IContentStore contentStore,
			NiceUrlProvider niceUrlResolver)
        {
			this.UmbracoContext = umbracoContext;
			this.DocumentLookups = documentLookups;
			DocumentLastChanceLookup = documentLastChanceLookup;
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
		internal IEnumerable<IDocumentLookup> DocumentLookups { get; private set; }

		/// <summary>
		/// Gets the last chance lookup
		/// </summary>
		internal IDocumentLastChanceLookup DocumentLastChanceLookup { get; private set; }

		/// <summary>
		/// Gets the content store.
		/// </summary>
		internal IContentStore ContentStore { get; private set; }

		/// <summary>
		/// Gets the nice urls provider.
		/// </summary>
		internal NiceUrlProvider NiceUrlProvider { get; private set; }
    }
}