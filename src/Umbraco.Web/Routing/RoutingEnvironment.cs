using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Routing
{
    
	/// <summary>
	/// represents a request for one specified Umbraco document to be rendered by one specified template, 
	/// using one particular culture.
	/// </summary>
    internal class RoutingEnvironment
    {
        public RoutingEnvironment(
            RouteLookups lookups,
            ILookupNotFound lookupNotFound,
            ContentStore contentStore)
        {
            RouteLookups = lookups;
            LookupNotFound = lookupNotFound;
            ContentStore = contentStore;
        }

		public RouteLookups RouteLookups { get; private set; }

    	public ILookupNotFound LookupNotFound { get; private set; }

        public ContentStore ContentStore { get; private set; }

    }
}