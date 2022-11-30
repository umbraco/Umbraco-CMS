using System.Globalization;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
///     We need this property converter so that we always force the value of a label to be a string
/// </summary>
/// <remarks>
///     Without a property converter defined for the label type, the value will be converted with
///     the `ConvertUsingDarkMagic` method which will try to parse the value into it's correct type, but this
///     can cause issues if the string is detected as a number and then strips leading zeros.
///     Example: http://issues.umbraco.org/issue/U4-7929
/// </remarks>
[DefaultPropertyValueConverter]
public class LabelValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
        => Constants.PropertyEditors.Aliases.Label.Equals(propertyType.EditorAlias);

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType)
    {
        LabelConfiguration? valueType =
            ConfigurationEditor.ConfigurationAs<LabelConfiguration>(propertyType.DataType.Configuration);
        switch (valueType?.ValueType)
        {
            case ValueTypes.DateTime:
            case ValueTypes.Date:
                return typeof(DateTime);
            case ValueTypes.Time:
                return typeof(TimeSpan);
            case ValueTypes.Decimal:
                return typeof(decimal);
            case ValueTypes.Integer:
                return typeof(int);
            case ValueTypes.Bigint:
                return typeof(long);
            default: // everything else is a string
                return typeof(string);
        }
    }

    public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
        => PropertyCacheLevel.Element;

    public override object ConvertSourceToIntermediate(IPublishedElement owner, IPublishedPropertyType propertyType, object? source, bool preview)
    {
        LabelConfiguration? valueType =
            ConfigurationEditor.ConfigurationAs<LabelConfiguration>(propertyType.DataType.Configuration);
        switch (valueType?.ValueType)
        {
            case ValueTypes.DateTime:
            case ValueTypes.Date:
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
            case ValueTypes.Time:
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
            case ValueTypes.Decimal:
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
            case ValueTypes.Integer:
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
            case ValueTypes.Bigint:
                if (source is string sourceLongString)
                {
                    return long.TryParse(sourceLongString, out var i) ? i : 0;
                }

                return 0L;
            default: // everything else is a string
                return source?.ToString() ?? string.Empty;
        }
    }
}
