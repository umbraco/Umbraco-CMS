using System.Linq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models
{
    public static class MediaExtensions
    {
        /// <summary>
        /// Gets the URL of a media item.
        /// </summary>
        public static string GetUrl(this IMedia media, string propertyAlias, MediaUrlGeneratorCollection mediaUrlGenerators)
        {
            if (!media.Properties.TryGetValue(propertyAlias, out var property))
                return string.Empty;

            // TODO: would need to be adjusted to variations, when media become variants
            if (mediaUrlGenerators.TryGetMediaPath(property.PropertyType.PropertyEditorAlias, property.GetValue(), out var mediaUrl))
            {
                return mediaUrl;
            }

            // Without knowing what it is, just adding a string here might not be very nice
            return string.Empty;
        }

        /// <summary>
        /// Gets the URLs of a media item.
        /// </summary>
        public static string[] GetUrls(this IMedia media, ContentSettings contentSettings, MediaUrlGeneratorCollection mediaUrlGenerators)
        {
            return contentSettings.Imaging.AutoFillImageProperties
                .Select(field => media.GetUrl(field.Alias, mediaUrlGenerators))
                .Where(link => string.IsNullOrWhiteSpace(link) == false)
                .ToArray();
        }
    }
}
