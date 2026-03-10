using NPoco;
using NPoco.DatabaseTypes;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class NPocoDatabaseTypeExtensions
{
    [Obsolete("Usage of this method indicates a code smell.")]
    public static bool IsSqlServer(this IDatabaseType databaseType)
        => databaseType is not null && databaseType.GetProviderName() == "Microsoft.Data.SqlClient";

    [Obsolete("Usage of this method indicates a code smell.")]
    public static bool IsSqlite(this IDatabaseType databaseType)
        => databaseType is SQLiteDatabaseType;
}
