using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class DictionaryLanguageTextRelator
    {
        internal DictionaryDto Current;

        internal DictionaryDto Map(DictionaryDto a, LanguageTextDto p)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            // Is this the same DictionaryItem as the current one we're processing
            if (Current != null && Current.UniqueId == a.UniqueId)
            {
                // Yes, just add this LanguageTextDto to the current DictionaryItem's collection
                Current.LanguageTextDtos.Add(p);

                // Return null to indicate we're not done with this DictionaryItem yet
                return null;
            }

            // This is a different DictionaryItem to the current one, or this is the 
            // first time through and we don't have a Tab yet

            // Save the current DictionaryItem
            var prev = Current;

            // Setup the new current DictionaryItem
            Current = a;
            Current.LanguageTextDtos = new List<LanguageTextDto>();
            Current.LanguageTextDtos.Add(p);

            // Return the now populated previous DictionaryItem (or null if first time through)
            return prev;
        }
    }
}