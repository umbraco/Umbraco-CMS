using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Used to do some type conversions from ImageCropDataSet to string and JObject
    /// </summary>
    public class ImageCropDataSetConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            var convertableTypes = new[]
            {
                typeof(JObject)
            };

            return convertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
                   || base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertTo(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value,
            Type destinationType)
        {
            var cropDataSet = value as ImageCropDataSet;
            if (cropDataSet == null)
                return null;

            //JObject
            if (TypeHelper.IsTypeAssignableFrom<JObject>(destinationType))
            {
                return JObject.FromObject(cropDataSet);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}