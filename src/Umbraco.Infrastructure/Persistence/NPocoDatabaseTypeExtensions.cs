using NPoco;
using NPoco.DatabaseTypes;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class NPocoDatabaseTypeExtensions
{
    /// <summary>
    /// Returns a value indicating whether the specified <paramref name="databaseType"/> represents a SQL Server database provider.
    /// </summary>
    /// <param name="databaseType">The database type to evaluate.</param>
    /// <returns><c>true</c> if the provider is SQL Server; otherwise, <c>false</c>.</returns>
    [Obsolete("Usage of this method indicates a code smell.")]
    public static bool IsSqlServer(this IDatabaseType databaseType)
        => databaseType is not null && databaseType.GetProviderName() == "Microsoft.Data.SqlClient";

    /// <summary>
    /// Determines whether the specified database type is SQLite.
    /// </summary>
    /// <param name="databaseType">The database type to check.</param>
    /// <returns><c>true</c> if the database type is SQLite; otherwise, <c>false</c>.</returns>
    [Obsolete("Usage of this method indicates a code smell.")]
    public static bool IsSqlite(this IDatabaseType databaseType)
        => databaseType is SQLiteDatabaseType;
}
