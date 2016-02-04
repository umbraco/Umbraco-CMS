using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.Core.Services;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Used to strongly type the value for the image cropper
    /// </summary>
    [DefaultPropertyValueConverter(typeof (JsonValueConverter))] //this shadows the JsonValueConverter
    [PropertyValueType(typeof (ImageCropDataSet))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class ImageCropperValueConverter : Core.PropertyEditors.ValueConverters.ImageCropperValueConverter
    {
        public ImageCropperValueConverter()
        {
        }

        public ImageCropperValueConverter(IDataTypeService dataTypeService) : base(dataTypeService)
        {
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            var baseVal = base.ConvertDataToSource(propertyType, source, preview);
            var json = baseVal as JObject;
            if (json == null) return baseVal;

            var serializer = new JsonSerializer
            {
                Culture = CultureInfo.InvariantCulture,
                FloatParseHandling = FloatParseHandling.Decimal
            };

            //return the strongly typed model
            return json.ToObject<ImageCropDataSet>(serializer);
        }
    }
}