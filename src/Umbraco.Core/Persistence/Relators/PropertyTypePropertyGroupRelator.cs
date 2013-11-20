using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class PropertyTypePropertyGroupRelator
    {
        internal MemberTypeReadOnlyDto Current;

        internal MemberTypeReadOnlyDto Map(MemberTypeReadOnlyDto a, PropertyTypeReadOnlyDto p, PropertyTypeGroupReadOnlyDto g)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            // Is this the same MemberTypeReadOnlyDto as the current one we're processing
            if (Current != null && Current.UniqueId == a.UniqueId)
            {
                //This property may already be added so we need to check for that
                if (p.Id.HasValue && Current.PropertyTypes.Any(x => x.Id == p.Id.Value) == false)
                {
                    // Add this PropertyTypeReadOnlyDto to the current MemberTypeReadOnlyDto's collection
                    Current.PropertyTypes.Add(p);
                }
                
                if (g.Id.HasValue && Current.PropertyTypeGroups != null && Current.PropertyTypeGroups.Any(x => x.Id == g.Id.Value) == false)
                {
                    Current.PropertyTypeGroups.Add(g);
                }

                // Return null to indicate we're not done with this MemberTypeReadOnlyDto yet
                return null;
            }

            // This is a different MemberTypeReadOnlyDto to the current one, or this is the 
            // first time through and we don't have a Tab yet

            // Save the current MemberTypeReadOnlyDto
            var prev = Current;

            // Setup the new current MemberTypeReadOnlyDto
            Current = a;
            Current.PropertyTypes = new List<PropertyTypeReadOnlyDto>();
            //this can be null since we are doing a left join
            if (p.Id.HasValue)
            {
                Current.PropertyTypes.Add(p);
            }

            Current.PropertyTypeGroups = new List<PropertyTypeGroupReadOnlyDto>();
            if (g.Id.HasValue)
            {
                Current.PropertyTypeGroups.Add(g);
            }

            // Return the now populated previous MemberTypeReadOnlyDto (or null if first time through)
            return prev;
        }
    }
}