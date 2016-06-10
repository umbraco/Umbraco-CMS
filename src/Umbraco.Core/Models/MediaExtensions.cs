using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Core.Models
{
    internal static class MediaExtensions
    {
        /// <summary>
        /// Hack: we need to put this in a real place, this is currently just used to render the urls for a media item in the back office
        /// </summary>
        /// <returns></returns>
        public static string GetUrl(this IMedia media, string propertyAlias, ILogger logger)
        {
            var propertyType = media.PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyAlias));
            if (propertyType != null)
            {
                var val = media.Properties[propertyType];
                if (val != null)
                {
                    var jsonString = val.Value as string;
                    if (jsonString != null)
                    {
                        if (propertyType.PropertyEditorAlias == Constants.PropertyEditors.ImageCropperAlias)
                        {
                            if (jsonString.DetectIsJson())
                            {
                                try
                                {
                                    var json = JsonConvert.DeserializeObject<JObject>(jsonString);
                                    if (json["src"] != null)
                                    {
                                        return json["src"].Value<string>();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Error<ImageCropperValueConverter>("Could not parse the string " + jsonString + " to a json object", ex);
                                    return string.Empty;
                                }
                            }
                            else
                            {
                                return jsonString;
                            }
                        }
                        else if (propertyType.PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias)
                        {
                            return jsonString;
                        }
                        //hrm, without knowing what it is, just adding a string here might not be very nice
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Hack: we need to put this in a real place, this is currently just used to render the urls for a media item in the back office
        /// </summary>
        /// <returns></returns>
        public static string[] GetUrls(this IMedia media, IContentSection contentSection, ILogger logger)
        {
            var links = new List<string>();
            var autoFillProperties = contentSection.ImageAutoFillProperties.ToArray();
            if (autoFillProperties.Any())
            {
                links.AddRange(
                    autoFillProperties
                        .Select(field => media.GetUrl(field.Alias, logger))
                        .Where(link => link.IsNullOrWhiteSpace() == false));
            }
            return links.ToArray();
        }
    }
}