using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    //internal class TaskUserRelator
    //{
    //    internal TaskDto Current;

    //    internal TaskDto Map(TaskDto a, UserDto p)
    //    {
    //        // Terminating call.  Since we can return null from this function
    //        // we need to be ready for PetaPoco to callback later with null
    //        // parameters
    //        if (a == null)
    //            return Current;

    //        // Is this the same TaskDto as the current one we're processing
    //        if (Current != null && Current.Id == a.Id)
    //        {
    //            // Yes, set the user 
    //            Current.MacroPropertyDtos.Add(p);

    //            // Return null to indicate we're not done with this Macro yet
    //            return null;
    //        }

    //        // This is a different Macro to the current one, or this is the 
    //        // first time through and we don't have one yet

    //        // Save the current Macro
    //        var prev = Current;

    //        // Setup the new current Macro
    //        Current = a;
    //        Current.MacroPropertyDtos = new List<MacroPropertyDto>();
    //        //this can be null since we are doing a left join
    //        if (p.Alias != null)
    //        {
    //            Current.MacroPropertyDtos.Add(p);
    //        }

    //        // Return the now populated previous Macro (or null if first time through)
    //        return prev;
    //    }
    //}

    internal class MacroPropertyRelator
    {
        internal MacroDto Current;

        internal MacroDto Map(MacroDto a, MacroPropertyDto p)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            // Is this the same DictionaryItem as the current one we're processing
            if (Current != null && Current.Id == a.Id)
            {
                // Yes, just add this MacroPropertyDtos to the current item's collection
                Current.MacroPropertyDtos.Add(p);

                // Return null to indicate we're not done with this Macro yet
                return null;
            }

            // This is a different Macro to the current one, or this is the 
            // first time through and we don't have one yet

            // Save the current Macro
            var prev = Current;

            // Setup the new current Macro
            Current = a;
            Current.MacroPropertyDtos = new List<MacroPropertyDto>();
            //this can be null since we are doing a left join
            if (p.Alias != null)
            {
                Current.MacroPropertyDtos.Add(p);
            }

            // Return the now populated previous Macro (or null if first time through)
            return prev;
        }
    }
}