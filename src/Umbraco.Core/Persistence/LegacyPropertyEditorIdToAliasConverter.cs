using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Core.Persistence
{

    /// <summary>
    /// This is used to map old legacy property editor GUID's to the new Property Editor alias (string) format.
    /// </summary>
    /// <remarks>
    /// This can be used by developers on application startup to register a mapping from their old ids to their new aliases and vice-versa.
    /// </remarks>
    public static class LegacyPropertyEditorIdToAliasConverter
    {
        /// <summary>
        /// The map consists of a key which is always the GUID (lowercase, no hyphens + alias (trimmed))
        /// </summary>
        private static readonly ConcurrentDictionary<string, Tuple<Guid, string>> Map = new ConcurrentDictionary<string, Tuple<Guid, string>>(); 

        /// <summary>
        /// Creates a map for the specified legacy id and property editor alias
        /// </summary>
        /// <param name="legacyId"></param>
        /// <param name="alias"></param>
        /// <returns>true if the map was created or false if it was already created</returns>
        public static bool CreateMap(Guid legacyId, string alias)
        {
            var key = legacyId.ToString("N").ToLowerInvariant() + alias.Trim();
            return Map.TryAdd(key, new Tuple<Guid, string>(legacyId, alias));
        }

        /// <summary>
        /// Gets an alias based on the legacy ID
        /// </summary>
        /// <param name="legacyId"></param>
        /// <param name="throwIfNotFound">if set to true will throw an exception if the map isn't found</param>
        /// <returns>Returns the alias if found otherwise null if not found</returns>
        public static string GetAliasFromLegacyId(Guid legacyId, bool throwIfNotFound = false)
        {
            var found = Map.SingleOrDefault(x => x.Value.Item1 == legacyId);
            if (found.Equals(default(KeyValuePair<string, Tuple<Guid, string>>)))
            {
                if (throwIfNotFound)
                {
                    throw new ObjectNotFoundException("Could not find a map for a property editor with a legacy id of " + legacyId);
                }
                return null;
            }
            return found.Value.Item2;
        }

        /// <summary>
        /// Gets a legacy Id based on the alias
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="throwIfNotFound">if set to true will throw an exception if the map isn't found</param>
        /// <returns>Returns the legacy GUID of a property editor if found, otherwise returns null</returns>
        public static Guid? GetLegacyIdFromAlias(string alias, bool throwIfNotFound = false)
        {
            var found = Map.SingleOrDefault(x => x.Value.Item2 == alias);
            if (found.Equals(default(KeyValuePair<string, Tuple<Guid, string>>)))
            {
                if (throwIfNotFound)
                {
                    throw new ObjectNotFoundException("Could not find a map for a property editor with an alias of " + alias);
                }
                return null;
            }
            return found.Value.Item1;
        }
        

    }
}
