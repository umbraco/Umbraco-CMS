using AutoMapper;

namespace Umbraco.Web.Models.Mapping
{
    // FIXME KILLE THIS CLASS
    /// <summary>
    /// Provides extension methods for AutoMapper's <see cref="IMappingOperationOptions"/>.
    /// </summary>
    internal static class MappingOperationOptionsExtensions
    {
        private const string CultureKey = "MappingOperationOptions.Culture";
        private const string IncludedPropertiesKey = "MappingOperationOptions.IncludeProperties";

        /// <summary>
        /// Gets the context culture.
        /// </summary>
        public static string GetCulture(this IMappingOperationOptions options)
        {
            return options.Items.TryGetValue(CultureKey, out var obj) && obj is string s ? s : null;
        }

        /// <summary>
        /// Sets a context culture.
        /// </summary>
        public static void SetCulture(this IMappingOperationOptions options, string culture)
        {
            options.Items[CultureKey] = culture;
        }

        /// <summary>
        /// Get included properties.
        /// </summary>
        public static string[] GetIncludedProperties(this IMappingOperationOptions options)
        {
            return options.Items.TryGetValue(IncludedPropertiesKey, out var obj) && obj is string[] s ? s : null;
        }
    }
}
