using NPoco;
using NPoco.DatabaseTypes;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class NPocoDatabaseTypeExtensions
{
    [Obsolete("Usage of this method indicates a code smell.")]
    public static bool IsSqlServer(this DatabaseType databaseType) =>

        // note that because SqlServerDatabaseType is the base class for
        // all Sql Server types eg SqlServer2012DatabaseType, this will
        // test *any* version of Sql Server.
        databaseType is SqlServerDatabaseType;

    [Obsolete("Usage of this method indicates a code smell.")]
    public static bool IsSqlite(this DatabaseType databaseType)
        => databaseType is SQLiteDatabaseType;
}
