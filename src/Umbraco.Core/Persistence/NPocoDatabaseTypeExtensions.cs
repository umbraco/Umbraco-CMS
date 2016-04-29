using NPoco;

namespace Umbraco.Core.Persistence
{
    internal static class NPocoDatabaseTypeExtensions
    {
        public static bool IsMySql(this DatabaseType databaseType)
        {
            return databaseType is NPoco.DatabaseTypes.MySqlDatabaseType;
        }

        public static bool IsSqlServer(this DatabaseType databaseType)
        {
            // note that because SqlServerDatabaseType is the base class for
            // all Sql Server types eg SqlServer2012DatabaseType, this will
            // test *any* version of Sql Server.
            return databaseType is NPoco.DatabaseTypes.SqlServerDatabaseType;
        }

        public static bool IsSqlCe(this DatabaseType databaseType)
        {
            return databaseType is NPoco.DatabaseTypes.SqlServerCEDatabaseType;
        }

        public static bool IsSqlServerOrCe(this DatabaseType databaseType)
        {
            return databaseType.IsSqlServer() || databaseType.IsSqlCe();
        }
    }
}
