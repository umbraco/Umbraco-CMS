using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods for <c>IPublishedProperty</c>.
    /// </summary>
    public static class PublishedPropertyExtension
    {
        // see notes in PublishedElementExtensions
        //
        private static IPublishedValueFallback PublishedValueFallback => Current.PublishedValueFallback;

        #region Value

        public static object Value(this IPublishedProperty property, string culture = null, string segment = null, Fallback fallback = default, object defaultValue = default)
        {
            return property.Value(PublishedValueFallback, culture, segment, fallback, defaultValue);
        }

        #endregion

        #region Value<T>

        public static T Value<T>(this IPublishedProperty property, string culture = null, string segment = null, Fallback fallback = default, T defaultValue = default)
        {
            return property.Value<T>(PublishedValueFallback, culture, segment, fallback, defaultValue);            
        }

        #endregion
    }
}
