namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains database provider name constants.
    /// </summary>
    public static class ProviderNames
    {
        /// <summary>
        ///     The provider name for SQLite databases.
        /// </summary>
        public const string SQLLite = "Microsoft.Data.Sqlite";

        /// <summary>
        ///     The provider name for SQL Server databases.
        /// </summary>
        public const string SQLServer = "Microsoft.Data.SqlClient";
    }
}
