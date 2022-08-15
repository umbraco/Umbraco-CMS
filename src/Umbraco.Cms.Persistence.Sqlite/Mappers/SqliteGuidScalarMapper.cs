using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

public class SqliteGuidScalarMapper : ScalarMapper<Guid>
{
    protected override Guid Map(object value)
        => Guid.Parse($"{value}");
}

public class SqliteNullableGuidScalarMapper : ScalarMapper<Guid?>
{
    protected override Guid? Map(object? value)
    {
        if (value is null || value == DBNull.Value)
        {
            return default;
        }

        return Guid.TryParse($"{value}", out Guid result)
            ? result
            : default(Guid?);
    }
}
