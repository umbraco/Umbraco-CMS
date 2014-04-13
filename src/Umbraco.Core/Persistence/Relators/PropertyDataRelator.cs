using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class PropertyDataRelator
    {
        internal MemberReadOnlyDto Current;

        internal MemberReadOnlyDto Map(MemberReadOnlyDto a, PropertyDataReadOnlyDto p)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            p.VersionId = a.VersionId;

            // Is this the same MemberReadOnlyDto as the current one we're processing
            if (Current != null && Current.UniqueId == a.UniqueId)
            {
                //This property may already be added so we need to check for that
                if (Current.Properties.Any(x => x.Id == p.Id) == false)
                {
                    // Yes, just add this PropertyDataReadOnlyDto to the current MemberReadOnlyDto's collection
                    Current.Properties.Add(p);
                }                

                // Return null to indicate we're not done with this MemberReadOnlyDto yet
                return null;
            }

            // This is a different MemberReadOnlyDto to the current one, or this is the 
            // first time through and we don't have a Tab yet

            // Save the current MemberReadOnlyDto
            var prev = Current;

            // Setup the new current MemberReadOnlyDto
            Current = a;
            Current.Properties = new List<PropertyDataReadOnlyDto>();
            Current.Properties.Add(p);

            // Return the now populated previous MemberReadOnlyDto (or null if first time through)
            return prev;
        }
    }
}