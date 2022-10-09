using NPoco;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

public class SqlitePocoGuidMapper : DefaultMapper
{
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
        if (destType == typeof(Guid))
        {
            return value =>
            {
                var result = Guid.Parse($"{value}");
                return result;
            };
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
