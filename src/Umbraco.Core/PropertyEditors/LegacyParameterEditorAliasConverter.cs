using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Used to map the legacy parameter editor aliases to the new ones, this is really just used during 
    /// installation but has been put in a separate class in case we need it for other purposes
    /// </summary>
    public static class LegacyParameterEditorAliasConverter
    {
        /// <summary>
        /// The map consists of a key which is always the legacy alias + new alias (trimmed))
        /// </summary>
        private static ConcurrentDictionary<string, Tuple<string, string>> _map = new ConcurrentDictionary<string, Tuple<string, string>>();

        /// <summary>
        /// Creates a map for the specified legacy alias and property editor alias
        /// </summary>
        /// <param name="legacyAlias"></param>
        /// <param name="alias"></param>
        /// <returns>true if the map was created or false if it was already created</returns>
        public static bool CreateMap(string legacyAlias, string alias)
        {
            var key = legacyAlias.ToLowerInvariant() + alias.Trim();
            return _map.TryAdd(key, new Tuple<string, string>(legacyAlias, alias));
        }

        /// <summary>
        /// Gets an alias based on the legacy alias
        /// </summary>
        /// <param name="legacyAlias"></param>
        /// <param name="throwIfNotFound">if set to true will throw an exception if the map isn't found</param>
        /// <returns>Returns the alias if found otherwise null if not found</returns>
        public static string GetNewAliasFromLegacyAlias(string legacyAlias, bool throwIfNotFound = false)
        {
            var found = _map.FirstOrDefault(x => x.Value.Item1 == legacyAlias);
            if (found.Equals(default(KeyValuePair<string, Tuple<string, string>>)))
            {
                if (throwIfNotFound)
                {
                    throw new ObjectNotFoundException("Could not find a map for a property editor with a legacy alias of " + legacyAlias);
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
        public static string GetLegacyAliasFromNewAlias(string alias, bool throwIfNotFound = false)
        {
            var found = _map.FirstOrDefault(x => x.Value.Item2 == alias);
            if (found.Equals(default(KeyValuePair<string, Tuple<string, string>>)))
            {
                if (throwIfNotFound)
                {
                    throw new ObjectNotFoundException("Could not find a map for a property editor with an alias of " + alias);
                }
                return null;
            }
            return found.Value.Item1;
        }

        internal static int Count()
        {
            return _map.Count;
        }

        internal static void Reset()
        {
            _map = new ConcurrentDictionary<string, Tuple<string, string>>();
        }

        /// <summary>
        /// A method that should be called on startup to register the mappings for the internal core editors
        /// </summary>
        internal static void CreateMappingsForCoreEditors()
        {
            //All of these map to the content picker
            CreateMap("contentSubs", Constants.PropertyEditors.ContentPickerAlias);
            CreateMap("contentRandom", Constants.PropertyEditors.ContentPickerAlias);
            CreateMap("contentPicker", Constants.PropertyEditors.ContentPickerAlias);
            CreateMap("contentTree", Constants.PropertyEditors.ContentPickerAlias);
            CreateMap("contentAll", Constants.PropertyEditors.ContentPickerAlias);

            CreateMap("textMultiLine", Constants.PropertyEditors.TextboxMultipleAlias);
            CreateMap("text", Constants.PropertyEditors.TextboxAlias);
            CreateMap("bool", Constants.PropertyEditors.TrueFalseAlias);

            CreateMap("mediaCurrent", Constants.PropertyEditors.MediaPickerAlias);

            CreateMap("number", Constants.PropertyEditors.IntegerAlias);   
        }
    }
}