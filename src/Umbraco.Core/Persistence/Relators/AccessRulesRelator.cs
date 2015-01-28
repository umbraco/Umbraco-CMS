using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Relators
{
    internal class AccessRulesRelator
    {
        internal AccessDto Current;

        internal AccessDto Map(AccessDto a, AccessRuleDto p)
        {
            // Terminating call.  Since we can return null from this function
            // we need to be ready for PetaPoco to callback later with null
            // parameters
            if (a == null)
                return Current;

            // Is this the same AccessDto as the current one we're processing
            if (Current != null && Current.Id == a.Id)
            {
                // Yes, just add this AccessRuleDto to the current AccessDto's collection
                if (p.Id != default(Guid))
                {
                    Current.Rules.Add(p);    
                }
                

                // Return null to indicate we're not done with this AccessDto yet
                return null;
            }

            // This is a different AccessDto to the current one, or this is the 
            // first time through and we don't have a Tab yet

            // Save the current AccessDto
            var prev = Current;

            // Setup the new current AccessDto
            Current = a;
            Current.Rules = new List<AccessRuleDto>();
            if (p.Id != default(Guid))
            {
                Current.Rules.Add(p);    
            }

            // Return the now populated previous AccessDto (or null if first time through)
            return prev;
        }
    }
}