using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Routing
{
    
	/// <summary>
	/// represents a request for one specified Umbraco document to be rendered by one specified template, 
	/// using one particular culture.
	/// </summary>
    internal class RoutingContext
    {
        public RoutingContext(
			UmbracoContext umbracoContext,
			RequestDocumentResolversResolver requestDocumentResolversResolver,
            ContentStore contentStore,
			NiceUrlProvider niceUrlResolver)
        {
        	this.UmbracoContext = umbracoContext;
			this.RequestDocumentResolversResolver = requestDocumentResolversResolver;
            this.ContentStore = contentStore;
        	this.NiceUrlProvider = niceUrlResolver;
        }

		public UmbracoContext UmbracoContext { get; private set; }
		public RequestDocumentResolversResolver RequestDocumentResolversResolver { get; private set; }
        public ContentStore ContentStore { get; private set; }
		public NiceUrlProvider NiceUrlProvider { get; private set; }
    }
}