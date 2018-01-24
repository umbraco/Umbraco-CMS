using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    // fixme - this is VERY fucked up
    // see the SAME converter in the web project
    // there is ABSOLUTELY no reason to split the converter
    // no converters in WEB unless they absolutely need WEB, FFS

    /// <summary>
    /// Represents a value converter for the image cropper value editor.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class ImageCropperValueConverter : PropertyValueConverterBase
    {
        /// <inheritdoc />
        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ImageCropper);

        /// <inheritdoc />
        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (ImageCropperValue);

        /// <inheritdoc />
        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        private static void MergeConfiguration(ImageCropperValue value, PublishedDataType dataType)
        {
            var configuration = dataType.ConfigurationAs<ImageCropperEditorConfiguration>();
            MergeConfiguration(value, configuration);
        }

        // fixme why internal
        internal static void MergeConfiguration(ImageCropperValue value, IDataType dataType)
        {
            var configuration = dataType.ConfigurationAs<ImageCropperEditorConfiguration>();
            MergeConfiguration(value, configuration);
        }

        private static void MergeConfiguration(ImageCropperValue value, ImageCropperEditorConfiguration configuration)
        {
            // merge the crop values - the alias + width + height comes from
            // configuration, but each crop can store its own coordinates

            var configuredCrops = configuration.Crops;
            var crops = value.Crops.ToList();

            foreach (var configuredCrop in configuredCrops)
            {
                var crop = crops.FirstOrDefault(x => x.Alias == configuredCrop.Alias);
                if (crop != null)
                {
                    // found, apply the height & width
                    crop.Width = configuredCrop.Width;
                    crop.Height = configuredCrop.Height;
                }
                else
                {
                    // not found, add
                    crops.Add(new ImageCropperValue.ImageCropperCrop
                    {
                        Alias = configuredCrop.Alias,
                        Width = configuredCrop.Width,
                        Height = configuredCrop.Height
                    });
                }
            }

            // assume we don't have to remove the crops in value, that
            // are not part of configuration anymore?

            value.Crops = crops.ToArray();
        }

        /// <inheritdoc />
        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            ImageCropperValue value;
            try
            {
                value = JsonConvert.DeserializeObject<ImageCropperValue>(sourceString, new JsonSerializerSettings
                {
                    Culture = CultureInfo.InvariantCulture,
                    FloatParseHandling = FloatParseHandling.Decimal
                });
            }
            catch (Exception ex)
            {
                // cannot deserialize, assume it may be a raw image url
                Current.Logger.Error<ImageCropperValueConverter>($"Could not deserialize string \"{sourceString}\" into an image cropper value.", ex);
                value = new ImageCropperValue { Src = sourceString };
            }

            MergeConfiguration(value, propertyType.DataType);

            return value;
        }
    }
}
