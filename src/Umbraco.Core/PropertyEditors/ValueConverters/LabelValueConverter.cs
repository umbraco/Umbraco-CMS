using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     Provides property value conversion for the label property editors.
/// </summary>
/// <remarks>
///     <para>
///         There is one label editor per type of value a label can hold, so the type is taken from the editor the
///         property uses rather than from the configuration of its data type.
///     </para>
///     <para>
///         Without a property converter defined for the label type, the value would be converted with
///         the `ConvertUsingDarkMagic` method which will try to parse the value into it's correct type, but this
///         can cause issues if the string is detected as a number and then strips leading zeros.
///         Example: http://issues.umbraco.org/issue/U4-7929
///     </para>
/// </remarks>
[DefaultPropertyValueConverter]
public class LabelValueConverter : PropertyValueConverterBase
{
    /// <inheritdoc />
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias switch
        {
            Constants.PropertyEditors.Aliases.Label => true,
            Constants.PropertyEditors.Aliases.LabelText => true,
            Constants.PropertyEditors.Aliases.LabelInteger => true,
            Constants.PropertyEditors.Aliases.LabelBigInt => true,
            Constants.PropertyEditors.Aliases.LabelDecimal => true,
            Constants.PropertyEditors.Aliases.LabelDateTime => true,
            Constants.PropertyEditors.Aliases.LabelTime => true,
            _ => false,
        };

    /// <inheritdoc />
    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
        => propertyType.EditorAlias switch
        {
            Constants.PropertyEditors.Aliases.LabelInteger => typeof(int),
            Constants.PropertyEditors.Aliases.LabelBigInt => typeof(long),
            Constants.PropertyEditors.Aliases.LabelDecimal => typeof(decimal),
            Constants.PropertyEditors.Aliases.LabelDateTime => typeof(DateTime),
            Constants.PropertyEditors.Aliases.LabelTime => typeof(TimeSpan),
            _ => typeof(string),
        };

    /// <inheritdoc />
    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    /// <inheritdoc />
    public override object? ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
        => propertyType.EditorAlias switch
        {
            Constants.PropertyEditors.Aliases.LabelInteger => AsInteger(source),
            Constants.PropertyEditors.Aliases.LabelBigInt => AsBigInt(source),
            Constants.PropertyEditors.Aliases.LabelDecimal => AsDecimal(source),
            Constants.PropertyEditors.Aliases.LabelDateTime => AsDateTime(source),
            Constants.PropertyEditors.Aliases.LabelTime => AsTime(source),
            _ => source?.ToString() ?? string.Empty,
        };

    private static object AsDateTime(object? source)
    {
        if (source is DateTime sourceDateTime)
        {
            return sourceDateTime;
        }

        if (source is string sourceDateTimeString)
        {
            return DateTime.TryParse(sourceDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt)
                ? dt
                : DateTime.MinValue;
        }

        return DateTime.MinValue;
    }

    private static object AsTime(object? source)
    {
        if (source is DateTime sourceTime)
        {
            return sourceTime.TimeOfDay;
        }

        if (source is string sourceTimeString)
        {
            return TimeSpan.TryParse(sourceTimeString, CultureInfo.InvariantCulture, out TimeSpan ts)
                ? ts
                : TimeSpan.Zero;
        }

        return TimeSpan.Zero;
    }

    private static object AsDecimal(object? source)
    {
        if (source is decimal sourceDecimal)
        {
            return sourceDecimal;
        }

        if (source is string sourceDecimalString)
        {
            return decimal.TryParse(sourceDecimalString, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)
                ? d
                : 0;
        }

        if (source is double sourceDouble)
        {
            return Convert.ToDecimal(sourceDouble);
        }

        return 0M;
    }

    private static object AsInteger(object? source)
    {
        if (source is int sourceInt)
        {
            return sourceInt;
        }

        if (source is string sourceIntString)
        {
            return int.TryParse(sourceIntString, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)
                ? i
                : 0;
        }

        return 0;
    }

    private static object AsBigInt(object? source)
    {
        if (source is string sourceLongString)
        {
            return long.TryParse(sourceLongString, out var i) ? i : 0;
        }

        return 0L;
    }
}
