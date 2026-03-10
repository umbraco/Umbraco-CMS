using System.Globalization;
using NPoco;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

/// <summary>
/// Provides a custom POCO mapper for handling decimal values when working with SQLite databases.
/// </summary>
public class SqlitePocoDecimalMapper : DefaultMapper
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

        return base.GetFromDbConverter(destType, sourceType);
    }
}
