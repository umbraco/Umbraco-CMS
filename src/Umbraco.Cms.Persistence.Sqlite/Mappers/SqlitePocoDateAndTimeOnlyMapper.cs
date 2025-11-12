using NPoco;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;
public class SqlitePocoDateAndTimeOnlyMapper : DefaultMapper
{
    public override Func<object, object?> GetFromDbConverter(Type destType, Type sourceType)
    {
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
