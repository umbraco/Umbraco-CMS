using NPoco;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

/// <summary>
/// Provides a custom POCO mapper for handling GUID values when working with SQLite databases.
/// </summary>
public class SqlitePocoGuidMapper : DefaultMapper
{
    /// <inheritdoc/>
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
        if (destType == typeof(Guid))
        {
            return value => Guid.Parse($"{value}");
        }

        if (destType == typeof(Guid?))
        {
            return value =>
            {
                if (Guid.TryParse($"{value}", out Guid result))
                {
                    return result;
                }

                return default(Guid?);
            };
        }

        return base.GetFromDbConverter(destType, sourceType);
    }
}
