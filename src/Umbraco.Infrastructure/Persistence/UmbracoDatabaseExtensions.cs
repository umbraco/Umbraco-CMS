using NPoco;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class UmbracoDatabaseExtensions
{
    public static UmbracoDatabase AsUmbracoDatabase(this IUmbracoDatabase database)
    {
        if (database is not UmbracoDatabase asDatabase)
        {
            throw new Exception("oops: database.");
        }

        return asDatabase;
    }

    /// <summary>
    ///     Gets a dictionary of key/values directly from the database, no scope, nothing.
    /// </summary>
    /// <remarks>Used by <see cref="CoreRuntimeBootstrapper" /> to determine the runtime state.</remarks>
    public static IReadOnlyDictionary<string, string?>? GetFromKeyValueTable(
        this IUmbracoDatabase? database,
        string keyPrefix)
    {
        if (database is null)
        {
            return null;
        }

        // create the wildcard where clause
        ISqlSyntaxProvider sqlSyntax = database.SqlContext.SqlSyntax;
        var whereParam = sqlSyntax.GetStringColumnWildcardComparison(
            sqlSyntax.GetQuotedColumnName("key"),
            0,
            TextColumnType.NVarchar);

        Sql<ISqlContext>? sql = database.SqlContext.Sql()
            .Select<KeyValueDto>()
            .From<KeyValueDto>()
            .Where(whereParam, keyPrefix + sqlSyntax.GetWildcardPlaceholder());

        return database.Fetch<KeyValueDto>(sql)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    /// <summary>
    ///     Returns true if the database contains the specified table
    /// </summary>
    /// <param name="database"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static bool HasTable(this IUmbracoDatabase database, string tableName)
    {
        try
        {
            return database.SqlContext.SqlSyntax.GetTablesInSchema(database)
                .Any(table => table.InvariantEquals(tableName));
        }
        catch (Exception)
        {
            return false; // will occur if the database cannot connect
        }
    }

    /// <summary>
    ///     Returns true if the database contains no tables
    /// </summary>
    /// <param name="database"></param>
    /// <returns></returns>
    public static bool IsDatabaseEmpty(this IUmbracoDatabase database)
        => database.SqlContext.SqlSyntax.GetTablesInSchema(database).Any() == false;
}
