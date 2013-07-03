using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class UserSectionRelator
    {
        internal UserDto Current;

        internal UserDto Map(UserDto a, User2AppDto p)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            // Is this the same DictionaryItem as the current one we're processing
            if (Current != null && Current.Id == a.Id)
            {
                // Yes, just add this User2AppDto to the current item's collection
                Current.User2AppDtos.Add(p);

                // Return null to indicate we're not done with this User yet
                return null;
            }

            // This is a different User to the current one, or this is the 
            // first time through and we don't have one yet

            // Save the current User
            var prev = Current;

            // Setup the new current User
            Current = a;
            Current.User2AppDtos = new List<User2AppDto>();
            //this can be null since we are doing a left join
            if (p.AppAlias != null)
            {
                Current.User2AppDtos.Add(p);    
            }

            // Return the now populated previous User (or null if first time through)
            return prev;
        }
    }
}