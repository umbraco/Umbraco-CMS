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
            IEnumerable<ILookup> lookups,
            ILookupNotFound lookupNotFound,
            ContentStore contentStore)
        {
            Lookups = SortByPartWeight(lookups);
            LookupNotFound = lookupNotFound;
            ContentStore = contentStore;
        }

        private static IEnumerable<ILookup> SortByPartWeight(IEnumerable<ILookup> lookups)
        {
            return lookups.OrderBy(x =>
                {
                    var attribute = x.GetType().GetCustomAttributes(true).OfType<LookupWeightAttribute>().SingleOrDefault();
                    return attribute == null ? LookupWeightAttribute.DefaultWeight : attribute.Weight;
                }).ToList();
        }

    	public IEnumerable<ILookup> Lookups { get; private set; }

    	public ILookupNotFound LookupNotFound { get; private set; }

        public ContentStore ContentStore { get; private set; }

    }
}