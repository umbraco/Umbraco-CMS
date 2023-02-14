using System.Globalization;
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

        if (destType == typeof(decimal))
        {
            return value =>
            {
                var result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return result;
            };
        }

        if (destType == typeof(decimal?))
        {
            return value =>
            {
                var result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                return result;
            };
        }

        return base.GetFromDbConverter(destType, sourceType);
    }
}
