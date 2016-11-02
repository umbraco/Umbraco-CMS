using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Umbraco.Core.Dictionary;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Extension methods for ILocalizedTextService
    /// </summary>
    public static class LocalizedTextServiceExtensions
    {
        /// <summary>
        /// Localize using the current thread culture
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="key"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string Localize(this ILocalizedTextService manager, string key, string[] tokens)
        {
            return manager.Localize(key, Thread.CurrentThread.CurrentUICulture, tokens);
        }

        /// <summary>
        /// Localize using the current thread culture
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="key"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string Localize(this ILocalizedTextService manager, string key, IDictionary<string, string> tokens = null)
        {
            return manager.Localize(key, Thread.CurrentThread.CurrentUICulture, tokens);
        }

        /// <summary>
        /// Localize a key without any variables
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="key"></param>
        /// <param name="culture"></param>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static string Localize(this ILocalizedTextService manager, string key, CultureInfo culture, string[] tokens)
        {
            return manager.Localize(key, culture, ConvertToDictionaryVars(tokens));
        }

        /// <summary>
        /// Convert an array of strings to a dictionary of indicies -> values
        /// </summary>
        /// <param name="variables"></param>
        /// <returns></returns>
        internal static IDictionary<string, string> ConvertToDictionaryVars(string[] variables)
        {
            if (variables == null) return null;
            if (variables.Any() == false) return null;

            return variables.Select((s, i) => new { index = i.ToString(CultureInfo.InvariantCulture), value = s })
                .ToDictionary(keyvals => keyvals.index, keyvals => keyvals.value);
        }

        private static ICultureDictionary _cultureDictionary;

        /// <summary>
        /// TODO: We need to refactor how we work with ICultureDictionary - this is supposed to be the 'fast' way to
        /// do readonly access to the Dictionary without using the ILocalizationService. See TODO Notes in `DefaultCultureDictionary`
        /// Also NOTE that the ICultureDictionary is based on the ILocalizationService not the ILocalizedTextService (which is used
        /// only for the localization files - not the dictionary)
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string UmbracoDictionaryTranslate(this ILocalizedTextService manager, string text)
        {
            var cultureDictionary = CultureDictionary;
            return UmbracoDictionaryTranslate(text, cultureDictionary);
        }

        private static string UmbracoDictionaryTranslate(string text, ICultureDictionary cultureDictionary)
        {
            if (text == null)
                return null;

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return cultureDictionary[text].IfNullOrWhiteSpace(text);
        }

        private static ICultureDictionary CultureDictionary
        {
            get
            {
                return _cultureDictionary
                    ?? (_cultureDictionary = CultureDictionaryFactoryResolver.Current.Factory.CreateDictionary());
            }
        }
    }
}
