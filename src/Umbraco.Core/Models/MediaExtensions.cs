using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Core.Models
{
    public static class MediaExtensions
    {
        /// <summary>
        /// Gets the url of a media item.
        /// </summary>
        public static string GetUrl(this IMedia media, string propertyAlias, ILogger logger)
        {
            var propertyType = media.PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyAlias));
            if (propertyType == null) return string.Empty;

            var val = media.Properties[propertyType];
            if (val == null) return string.Empty;

            var jsonString = val.Value as string;
            if (jsonString == null) return string.Empty;

            if (propertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias)
                return jsonString;

            if (propertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias)
            {
                if (jsonString.DetectIsJson() == false)
                    return jsonString;

                try
                {
                    var json = JsonConvert.DeserializeObject<JObject>(jsonString);
                    if (json["src"] != null)
                        return json["src"].Value<string>();
                }
                catch (Exception ex)
                {
                    logger.Error<ImageCropperValueConverter>("Could not parse the string " + jsonString + " to a json object", ex);
                    return string.Empty;
                }
            }

            // hrm, without knowing what it is, just adding a string here might not be very nice
            return string.Empty;
        }

        /// <summary>
        /// Gets the urls of a media item.
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