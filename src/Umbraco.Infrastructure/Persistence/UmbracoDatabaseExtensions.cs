using System.Linq.Expressions;
using NPoco;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence;

internal static class UmbracoDatabaseExtensions
{
    /// <summary>
    /// Casts the given <see cref="IUmbracoDatabase"/> instance to an <see cref="UmbracoDatabase"/>.
    /// Throws an <see cref="Exception"/> if the cast is not possible.
    /// </summary>
    /// <param name="database">The database instance to cast.</param>
    /// <returns>The <see cref="UmbracoDatabase"/> instance.</returns>
    /// <exception cref="Exception">Thrown if <paramref name="database"/> is not an <see cref="UmbracoDatabase"/>.</exception>
    public static UmbracoDatabase AsUmbracoDatabase(this IUmbracoDatabase database)
    {
        if (database is not UmbracoDatabase asDatabase)
        {
            throw new Exception("oops: database.");
        }

        return asDatabase;
    }

    /// <summary>
    ///     Retrieves a dictionary of key/value pairs from the database table, filtered by a specified key prefix.
    ///     This method does not use a scope or transaction.
    /// </summary>
    /// <remarks>
    ///     Used by <see cref="RuntimeState" /> to determine the runtime state.
    ///     The query is performed directly against the database without any transactional scope.
    /// </remarks>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> instance to query. If null, the method returns null.</param>
    /// <param name="keyPrefix">The prefix used to filter the keys in the key/value table.</param>
    /// <returns>
    ///     A read-only dictionary containing key/value pairs whose keys start with the specified prefix, or <c>null</c> if <paramref name="database"/> is null.
    /// </returns>
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
            sqlSyntax.GetColumn(database.SqlContext.DatabaseType, KeyValueDto.TableName, "key", null),
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
    /// <param name="database">The database to check.</param>
    /// <param name="tableName">The table name to look for.</param>
    /// <returns>True if the table exists.</returns>
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
    /// <param name="database">The database to check.</param>
    /// <returns>True if the database has no tables.</returns>
    public static bool IsDatabaseEmpty(this IUmbracoDatabase database)
        => database.SqlContext.SqlSyntax.GetTablesInSchema(database).Any() == false;

    /// <summary>
    /// Executes a SQL query and returns the number of records it would return.
    /// </summary>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> instance to execute the query against.</param>
    /// <param name="sql">The <see cref="Sql"/> object representing the query to count results for.</param>
    /// <returns>The number of records matching the specified query.</returns>
    public static long Count(this IUmbracoDatabase database, Sql sql)
    {
        // We need to copy the sql into a new object, to avoid this method from changing the sql.
        var query = new Sql().Select("COUNT(*)").From().Append("(").Append(new Sql(sql.SQL, sql.Arguments)).Append(") as count_query");

        return database.ExecuteScalar<long>(query);
    }

    /// <summary>
    /// Asynchronously counts the number of records returned by the specified SQL query.
    /// </summary>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> instance to execute the query on.</param>
    /// <param name="sql">The <see cref="Sql"/> object representing the query to count records for. The query should select the records to be counted.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, with the result being the count of matching records as a <see cref="long"/>.</returns>
    public static async Task<long> CountAsync(this IUmbracoDatabase database, Sql sql)
    {
        // We need to copy the sql into a new object, to avoid this method from changing the sql.
        Sql query = new Sql().Select("COUNT(*)").From().Append("(").Append(new Sql(sql.SQL, sql.Arguments)).Append(") as count_query");

        return await database.ExecuteScalarAsync<long>(query);
    }

    /// <summary>
    /// Asynchronously retrieves a paged set of results from the database using the specified SQL query, applying sorting and mapping to each item.
    /// </summary>
    /// <typeparam name="TDto">The type of the data transfer object returned from the database query.</typeparam>
    /// <typeparam name="TResult">The type of the result after mapping.</typeparam>
    /// <param name="database">The <see cref="IUmbracoDatabase"/> instance to execute the query against.</param>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="skip">The number of records to skip before starting to return results.</param>
    /// <param name="take">The maximum number of records to return.</param>
    /// <param name="sortingAction">An action that applies sorting to the SQL query before execution.</param>
    /// <param name="mapper">A function that maps each <typeparamref name="TDto"/> instance to a <typeparamref name="TResult"/> instance.</param>
    /// <returns>A task representing the asynchronous operation. The result contains a <see cref="PagedModel{TResult}"/> with the total record count and the mapped items for the requested page.</returns>
    public static async Task<PagedModel<TResult>> PagedAsync<TDto, TResult>(
        this IUmbracoDatabase database,
        Sql<ISqlContext> sql,
        int skip,
        int take,
        Action<Sql<ISqlContext>> sortingAction,
        Func<TDto, TResult> mapper)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(skip, 0, nameof(skip));
        ArgumentOutOfRangeException.ThrowIfLessThan(take, 0, nameof(take));

        var count = await database.CountAsync(sql);
        if (take == 0 || skip >= count)
        {
            return new PagedModel<TResult>
            {
                Total = count,
                Items = [],
            };
        }

        sortingAction(sql);

        List<TDto> results = await database.SkipTakeAsync<TDto>(
            skip,
            take,
            sql);

        return new PagedModel<TResult>
        {
            Total = count,
            Items = results.Select(mapper),
        };
    }
}
