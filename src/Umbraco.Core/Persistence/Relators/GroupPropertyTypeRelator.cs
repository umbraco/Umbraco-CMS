using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class GroupPropertyTypeRelator
    {
        internal PropertyTypeGroupDto current;

        internal PropertyTypeGroupDto Map(PropertyTypeGroupDto a, PropertyTypeDto p, DataTypeDto d)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return current;

            //Set the PropertyTypeDto's DataTypeDto object
            if (p.DataTypeId == d.DataTypeId)
                p.DataTypeDto = d;

            // Is this the same Group as the current one we're processing
            if (current != null && current.Id == a.Id)
            {
                // Yes, just add this PropertyType to the current Group's collection of PropertyTypes
                current.PropertyTypeDtos.Add(p);

                // Return null to indicate we're not done with this Group yet
                return null;
            }

            // This is a different Group to the current one, or this is the 
            // first time through and we don't have a Tab yet

            // Save the current Group
            var prev = current;

            // Setup the new current Group
            current = a;
            current.PropertyTypeDtos = new List<PropertyTypeDto>();
            current.PropertyTypeDtos.Add(p);

            // Return the now populated previous Tab (or null if first time through)
            return prev;
        }

    }
}