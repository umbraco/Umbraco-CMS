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

        /// <summary>
        /// Tries to resolve a full 4 letter culture from a 2 letter culture name
        /// </summary>
        /// <param name="currentCulture">
        /// The culture to determine if it is only a 2 letter culture, if so we'll try to convert it, otherwise it will just be returned
        /// </param>
        /// <returns></returns>
        /// <remarks>
        /// TODO: This is just a hack due to the way we store the language files, they should be stored with 4 letters since that 
        /// is what they reference but they are stored with 2, further more our user's languages are stored with 2. So this attempts
        /// to resolve the full culture if possible.
        /// </remarks>
        CultureInfo ConvertToSupportedCultureWithRegionCode(CultureInfo currentCulture);
    }
}
