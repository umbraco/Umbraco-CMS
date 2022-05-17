using System.ComponentModel;
using System.Globalization;

namespace Umbraco.Cms.Core;

/// <summary>
///     Allows for converting string representations of 0 and 1 to boolean
/// </summary>
public class CustomBooleanTypeConverter : BooleanConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        if (sourceType == typeof(string))
        {
            return true;
        }

        return base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string str)
        {
            if (str == null || str.Length == 0 || str == "0")
            {
                return false;
            }

            if (str == "1")
            {
                return true;
            }

            if (str.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (str.Equals("No", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return base.ConvertFrom(context, culture, value);
    }
}
