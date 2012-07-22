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
            RouteLookups lookups,
            IRequestDocumentLastChanceResolver lookupNotFound,
            ContentStore contentStore,
			NiceUrlResolver niceUrlResolver)
        {
        	UmbracoContext = umbracoContext;
        	RouteLookups = lookups;
            LookupNotFound = lookupNotFound;
            ContentStore = contentStore;
        	NiceUrlResolver = niceUrlResolver;
        }

		public UmbracoContext UmbracoContext { get; private set; }
		public RouteLookups RouteLookups { get; private set; }
    	public IRequestDocumentLastChanceResolver LookupNotFound { get; private set; }
        public ContentStore ContentStore { get; private set; }
		public NiceUrlResolver NiceUrlResolver { get; private set; }
    }
}