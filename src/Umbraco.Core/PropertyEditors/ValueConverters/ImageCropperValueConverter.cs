using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// This ensures that the cropper config (pre-values/crops) are merged in with the front-end value.
    /// </summary>
    [DefaultPropertyValueConverter]
    public class ImageCropperValueConverter : PropertyValueConverterBase
    {
        private readonly IDataTypeService _dataTypeService;

        public override bool IsConverter(PublishedPropertyType propertyType)
            => propertyType.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.ImageCropper);

        public override Type GetPropertyValueType(PublishedPropertyType propertyType)
            => typeof (JToken);

        public override PropertyCacheLevel GetPropertyCacheLevel(PublishedPropertyType propertyType)
            => PropertyCacheLevel.Element;

        public ImageCropperValueConverter()
        {
            _dataTypeService = Current.Services.DataTypeService;
        }

        public ImageCropperValueConverter(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService ?? throw new ArgumentNullException(nameof(dataTypeService));
        }

        // represents the editor value (the one that is serialized to Json)
        internal class ImageCropperValue : ImageCropperEditorConfiguration
        {
            [JsonProperty("src")]
            public string Src { get; set; }
        }

        internal static void MergeConfiguration(ImageCropperValue value, IDataTypeService dataTypeService, int dataTypeId)
        {
            // merge the crop values - the alias + width + height comes from
            // configuration, but each crop can store its own coordinates

            var configuration = dataTypeService.GetDataType(dataTypeId).ConfigurationAs<ImageCropperEditorConfiguration>();
            var configuredCrops = configuration.Crops;
            var crops = value.Crops.ToList();

            foreach (var configuredCrop in configuredCrops)
            {
                var crop = crops.FirstOrDefault(x => x.Alias == configuredCrop.Alias);
                if (crop != null)
                {
                    crop.Width = configuredCrop.Width;
                    crop.Height = configuredCrop.Height;
                }
                else
                {
                    crops.Add(configuredCrop);
                }
            }

            value.Crops = crops.ToArray();
        }

        public override object ConvertSourceToIntermediate(IPublishedElement owner, PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            // not json, just return the string
            // fixme how can that even work?
            if (!sourceString.DetectIsJson())
                return sourceString;

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
                Current.Logger.Error<ImageCropperValueConverter>($"Could not deserialize string \"{sourceString}\" into an image cropper value.", ex);
                return sourceString;
            }

            MergeConfiguration(value, _dataTypeService, propertyType.DataType.Id);
            return value;
        }
    }
}
