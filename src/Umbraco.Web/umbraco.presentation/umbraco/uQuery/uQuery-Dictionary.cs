using umbraco;
using umbraco.cms.businesslogic;

namespace umbraco
{
    /// <summary>
    /// Static helper methods
    /// </summary>
    public static partial class uQuery
    {
        /// <summary>
        /// Gets a dictionary item if it exists. Otherwise returns the fallback string.
        /// </summary>
        /// <param name="key">The dictionary key.</param>
        /// <param name="fallback">The fallback.</param>
        /// <returns>
        /// A dictionary string or the fallback string
        /// </returns>
        public static string GetDictionaryItem(string key, string fallback)
        {
            if (Dictionary.DictionaryItem.hasKey(key))
            {
                var item = new Dictionary.DictionaryItem(key);
                return item.Value();
            }

            return fallback;
        }

        /// <summary>
        /// Gets the dictionary item for a specified language. Otherwise returns the fallback string.
        /// </summary>
        /// <param name="key">The dictionary key.</param>
        /// <param name="fallback">The fallback.</param>
        /// <param name="languageId">The language id.</param>
        /// <returns>
        /// Returns the value of a dictionary item from a language id, or the fallback string.
        /// </returns>
        public static string GetDictionaryItem(string key, string fallback, int languageId)
        {
            if (Dictionary.DictionaryItem.hasKey(key))
            {
                var item = new Dictionary.DictionaryItem(key);
                return item.Value(languageId);
            }

            return fallback;
        }
    }
}