using System;
using System.ComponentModel;
using System.Globalization;

namespace Umbraco.Core
{
    /// <summary>
    /// A custom type converter for UDI
    /// </summary>
    /// <remarks>
    /// Primarily this is used so that WebApi can auto-bind a string parameter to a UDI instance
    /// </remarks>
    internal class UdiTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                Udi udi;
                if (Udi.TryParse((string)value, out udi))
                {
                    return udi;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
