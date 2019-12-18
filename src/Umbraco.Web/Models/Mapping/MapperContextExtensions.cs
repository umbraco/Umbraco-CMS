using Umbraco.Core.Mapping;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Provides extension methods for the <see cref="MapperContext"/> class.
    /// </summary>
    internal static class MapperContextExtensions
    {
        private const string CultureKey = "Map.Culture";
        private const string IncludedPropertiesKey = "Map.IncludedProperties";

        /// <summary>
        /// Gets the context culture.
        /// </summary>
        public static string GetCulture(this MapperContext context)
        {
            return context.HasItems && context.Items.TryGetValue(CultureKey, out var obj) && obj is string s ? s : null;
        }

        /// <summary>
        /// Sets a context culture.
        /// </summary>
        public static void SetCulture(this MapperContext context, string culture)
        {
            context.Items[CultureKey] = culture;
        }

        /// <summary>
        /// Get included properties.
        /// </summary>
        public static string[] GetIncludedProperties(this MapperContext context)
        {
            return context.HasItems && context.Items.TryGetValue(IncludedPropertiesKey, out var obj) && obj is string[] s ? s : null;
        }

        /// <summary>
        /// Sets included properties.
        /// </summary>
        public static void SetIncludedProperties(this MapperContext context, string[] properties)
        {
            context.Items[IncludedPropertiesKey] = properties;
        }
    }
}