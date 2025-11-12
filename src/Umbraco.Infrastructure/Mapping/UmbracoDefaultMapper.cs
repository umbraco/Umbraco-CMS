using System.Globalization;
using NPoco;

namespace Umbraco.Cms.Core.Mapping;

/// <summary>
/// Provides default type conversion logic for mapping Umbraco database values to .NET types, extending the base mapping
/// behavior with support for additional types such as decimal, DateOnly, and TimeOnly.
/// </summary>
public class UmbracoDefaultMapper : DefaultMapper
{
    /// <inheritdoc/>
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
        if (destType == typeof(decimal))
        {
            return value => Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        if (destType == typeof(decimal?))
        {
            return value => Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }

        if (destType == typeof(DateOnly))
        {
            return value =>
            {
                if (value is DateTime dateTime)
                {
                    return DateOnly.FromDateTime(dateTime);
                }

                return DateOnly.Parse(value.ToString()!);
            };
        }

        if (destType == typeof(DateOnly?))
        {
            return value =>
            {
                if (value == null)
                {
                    return default(DateOnly?);
                }

                if (value is DateTime dateTime)
                {
                    return DateOnly.FromDateTime(dateTime);
                }

                return DateOnly.Parse(value.ToString()!);
            };
        }

        if (destType == typeof(TimeOnly))
        {
            return value =>
            {
                if (value is DateTime dateTime)
                {
                    return TimeOnly.FromDateTime(dateTime);
                }
                return TimeOnly.Parse(value.ToString()!);
            };
        }

        if (destType == typeof(TimeOnly?))
        {
            return value =>
            {
                if (value == null)
                {
                    return default(TimeOnly?);
                }

                if (value is DateTime dateTime)
                {
                    return TimeOnly.FromDateTime(dateTime);
                }

                return TimeOnly.Parse(value.ToString()!);
            };
        }

        return base.GetFromDbConverter(destType, sourceType);
    }
}
