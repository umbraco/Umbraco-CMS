using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors.ValueConverters
{
    /// <summary>
    /// Converts <see cref="ImageCropperValue"/> to string or JObject (why?).
    /// </summary>
    public class ImageCropperValueTypeConverter : TypeConverter
    {
        private static readonly Type[] ConvertableTypes = { typeof(JObject) };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ConvertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
                   || CanConvertFrom(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var cropperValue = value as ImageCropperValue;
            if (cropperValue == null)
                return null;

            return TypeHelper.IsTypeAssignableFrom<JObject>(destinationType)
                ? JObject.FromObject(cropperValue)
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
