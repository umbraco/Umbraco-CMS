using System.Globalization;

namespace Umbraco.Core.Services
{

    /// <summary>
    /// Extension methods for ILocalizedTextService
    /// </summary>
    public static class LocalizedTextServiceExtensions
    {
        /// <summary>
        /// Localize a key without any variables
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="key"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static string Localize(this ILocalizedTextService manager, string key, CultureInfo culture)
        {
            return manager.Localize(key, culture, null);
        }

    }
}
