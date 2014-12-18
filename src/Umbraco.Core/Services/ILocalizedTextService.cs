using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// The entry point to localize any key in the text storage source for a given culture 
    /// </summary>
    /// <remarks>
    /// This class is created to be as simple as possible so that it can be replaced very easily, 
    /// all other methods are extension methods that simply call the one underlying method in this class
    /// </remarks>
    public interface ILocalizedTextService
    {
        /// <summary>
        /// Localize a key with variables
        /// </summary>
        /// <param name="key"></param>
        /// <param name="culture"></param>
        /// <param name="tokens">This can be null</param>
        /// <returns></returns>
        string Localize(string key, CultureInfo culture, IDictionary<string, string> tokens = null);

        /// <summary>
        /// Returns all key/values in storage for the given culture
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> GetAllStoredValues(CultureInfo culture);

        /// <summary>
        /// Returns a list of all currently supported cultures
        /// </summary>
        /// <returns></returns>
        IEnumerable<CultureInfo> GetSupportedCultures();
    }
}
