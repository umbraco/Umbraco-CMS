using NPoco;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

/// <summary>
/// Provides a custom POCO mapper for handling date and time only values when working with SQLite databases.
/// </summary>
public class SqlitePocoDateAndTimeOnlyMapper : DefaultMapper
{
    /// <inheritdoc/>
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
        if (IsDateOnlyType(destType))
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
