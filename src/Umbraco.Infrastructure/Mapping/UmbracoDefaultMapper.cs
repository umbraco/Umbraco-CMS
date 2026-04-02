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

        if(IsDateOnlyType(destType))
        {
            return value => ConvertToDateOnly(value, IsNullableType(destType));
        }

        if (IsTimeOnlyType(destType))
        {
            return value => ConvertToTimeOnly(value, IsNullableType(destType));
        }

        return base.GetFromDbConverter(destType, sourceType);
    }

    private static bool IsDateOnlyType(Type type) =>
        type == typeof(DateOnly) || type == typeof(DateOnly?);

    private static bool IsTimeOnlyType(Type type) =>
        type == typeof(TimeOnly) || type == typeof(TimeOnly?);

    private static bool IsNullableType(Type type) =>
        Nullable.GetUnderlyingType(type) != null;

    private static object? ConvertToDateOnly(object? value, bool isNullable)
    {
        if (value is null)
        {
            return isNullable ? null : default(DateOnly);
        }

        if (value is DateTime dt)
        {
            return DateOnly.FromDateTime(dt);
        }

        return DateOnly.Parse(value.ToString()!);
    }

    private static object? ConvertToTimeOnly(object? value, bool isNullable)
    {
        if (value is null)
        {
            return isNullable ? null : default(TimeOnly);
        }

        if (value is DateTime dt)
        {
            return TimeOnly.FromDateTime(dt);
        }

        return TimeOnly.Parse(value.ToString()!);
    }
}
