using System;
using System.ComponentModel;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    /// <summary>
    /// Allows for converting string representations of 0 and 1 to boolean
    /// </summary>
    internal class CustomBooleanTypeConverter : BooleanConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                var str = (string)value;
                if (str == "1") return true;
                if (str == "0" || str == "") return false;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}