using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;
using Umbraco.Web.Models;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Used to do some type conversions from ImageCropDataSet to string and JObject
    /// </summary>
    public class ImageCropDataSetTypeConverter : TypeConverter
    {
        private static readonly Type[] ConvertableTypes = { typeof(JObject) };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ConvertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
                   || base.CanConvertFrom(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var cropDataSet = value as ImageCropDataSet;
            if (cropDataSet == null)
                return null;

            return TypeHelper.IsTypeAssignableFrom<JObject>(destinationType)
                ? JObject.FromObject(cropDataSet)
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
