// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Converts <see cref="ImageCropperValue" /> to string or JObject (why?).
/// </summary>
public class ImageCropperValueTypeConverter : TypeConverter
{
    private static readonly Type[] _convertableTypes = { typeof(JObject) };

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        if (destinationType is null)
        {
            return false;
        }

        return _convertableTypes.Any(x => TypeHelper.IsTypeAssignableFrom(x, destinationType))
               || CanConvertFrom(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (value is not ImageCropperValue cropperValue)
        {
            return null;
        }

        return TypeHelper.IsTypeAssignableFrom<JObject>(destinationType)
            ? JObject.FromObject(cropperValue)
            : base.ConvertTo(context, culture, value, destinationType);
    }
}
