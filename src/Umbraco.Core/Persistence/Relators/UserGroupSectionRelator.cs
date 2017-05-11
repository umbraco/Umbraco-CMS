using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class UserGroupSectionRelator
    {
        internal UserGroupDto Current;

        internal UserGroupDto Map(UserGroupDto a, UserGroup2AppDto p)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            // Is this the same object as the current one we're processing
            if (Current != null && Current.Id == a.Id)
            {
                if (p.AppAlias.IsNullOrWhiteSpace() == false)
                {
                    // Yes, just add this to the current item's collection
                    Current.UserGroup2AppDtos.Add(p);
                }

                // Return null to indicate we're not done with this User yet
                return null;
            }

            // This is a different object to the current one, or this is the 
            // first time through and we don't have one yet

            // Save the current instance
            var prev = Current;

            // Setup the new current instance
            Current = a;
            Current.UserGroup2AppDtos = new List<UserGroup2AppDto>();
            //this can be null since we are doing a left join
            if (p.AppAlias.IsNullOrWhiteSpace() == false)
            {
                Current.UserGroup2AppDtos.Add(p);
            }

            // Return the now populated previous user group (or null if first time through)
            return prev;
        }
    }
}