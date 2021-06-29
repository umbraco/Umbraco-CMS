using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Models
{
    public static class MediaExtensions
    {
        /// <summary>
        /// Gets the URL of a media item.
        /// </summary>
        public static string GetUrl(this IMedia media, string propertyAlias, ILogger logger)
        {
            if (!media.Properties.TryGetValue(propertyAlias, out var property))
                return string.Empty;

            if (Current.PropertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out var editor)
                && editor is IDataEditorWithMediaPath dataEditor)
            {
                // TODO: would need to be adjusted to variations, when media become variants
                var value = property.GetValue();
                return dataEditor.GetMediaPath(value);
            }

            // Without knowing what it is, just adding a string here might not be very nice
            return string.Empty;
        }

        /// <summary>
        /// Gets the URLs of a media item.
        /// </summary>
        public static string[] GetUrls(this IMedia media, IContentSection contentSection, ILogger logger)
        {
            return contentSection.ImageAutoFillProperties
                .Select(field => media.GetUrl(field.Alias, logger))
                .Where(link => string.IsNullOrWhiteSpace(link) == false)
                .ToArray();
        }
    }
}
