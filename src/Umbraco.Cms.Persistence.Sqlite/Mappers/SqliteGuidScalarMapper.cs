using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Persistence.Sqlite.Mappers;

/// <summary>
/// Maps GUID values from SQLite TEXT storage to .NET Guid type.
/// </summary>
public class SqliteGuidScalarMapper : ScalarMapper<Guid>
{
    /// <inheritdoc />
    protected override Guid Map(object value)
        => Guid.Parse($"{value}");
}

/// <summary>
/// Maps nullable GUID values from SQLite TEXT storage to .NET Guid? type.
/// </summary>
public class SqliteNullableGuidScalarMapper : ScalarMapper<Guid?>
{
    /// <inheritdoc />
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
