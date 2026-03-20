namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Contains database provider name constants.
    /// </summary>
    public static class ProviderNames
    {
        /// <summary>
        ///     The ADO.NET provider name for SQLite databases.
        /// </summary>
        public const string SQLite = "Microsoft.Data.Sqlite";

        /// <summary>
        ///     The ADO.NET provider name for SQL Server databases.
        /// </summary>
        public const string SQLServer = "Microsoft.Data.SqlClient";

        /// <summary>
        ///     Contains EF Core provider name constants, as returned by
        ///     <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.ProviderName"/>.
        /// </summary>
        public static class EFCore
        {
            /// <summary>
            ///     The EF Core provider name for SQLite databases.
            /// </summary>
            public const string SQLite = "Microsoft.EntityFrameworkCore.Sqlite";

            /// <summary>
            ///     The EF Core provider name for SQL Server databases.
            /// </summary>
            public const string SQLServer = "Microsoft.EntityFrameworkCore.SqlServer";
        }
    }
}
