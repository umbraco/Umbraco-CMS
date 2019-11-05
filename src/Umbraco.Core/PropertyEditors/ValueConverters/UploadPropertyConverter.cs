using System;
using Newtonsoft.Json;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// The upload property value converter.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class UploadPropertyConverter : PropertyValueConverterBase
    {
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.Equals(Constants.PropertyEditors.Aliases.UploadField);

        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (string);

        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public override object ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel cacheLevel, object source, bool preview)
        {
            var sourceValue = source?.ToString();

            if (sourceValue?.DetectIsJson() == true)
            {
                // this may be an image cropper value - let's try to convert it
                try
                {
                    return JsonConvert.DeserializeObject<ImageCropperValue>(sourceValue)?.Src;
                }
                catch
                {
                    // ignore, this can't be converted
                }
            }

            return sourceValue;
        }
    }
}
