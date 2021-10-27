using System;
using System.Globalization;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Represents a value converter for the image cropper value editor.
    /// </summary>
    [DefaultPropertyValueConverter(typeof(JsonValueConverter))]
    public class ImageCropperValueConverter : PropertyValueConverterBase
    {
        /// <inheritdoc />
        public override bool IsConverter(IPublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ImageCropper);

        /// <inheritdoc />
        public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
            => typeof (ImageCropperValue);

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        private static readonly JsonSerializerSettings ImageCropperValueJsonSerializerSettings = new JsonSerializerSettings
        {
            Culture = CultureInfo.InvariantCulture,
            FloatParseHandling = FloatParseHandling.Decimal
        };

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            ImageCropperValue value;
            try
            {
                value = JsonConvert.DeserializeObject<ImageCropperValue>(sourceString, ImageCropperValueJsonSerializerSettings);
            }
            catch (Exception ex)
            {
                // cannot deserialize, assume it may be a raw image url
                Current.Logger.Error<ImageCropperValueConverter, string>(ex, "Could not deserialize string '{JsonString}' into an image cropper value.", sourceString);
                value = new ImageCropperValue { Src = sourceString };
            }

            value?.ApplyConfiguration(propertyType.DataType.ConfigurationAs<ImageCropperConfiguration>());

            return value;
        }
    }
}
