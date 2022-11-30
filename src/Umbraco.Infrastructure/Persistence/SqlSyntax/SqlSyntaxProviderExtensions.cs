using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

internal enum WhereInType
{
    In,
    NotIn,
}

internal static class SqlSyntaxProviderExtensions
{
    public static IEnumerable<DbIndexDefinition>
        GetDefinedIndexesDefinitions(this ISqlSyntaxProvider sql, IDatabase db) =>
        sql.GetDefinedIndexes(db)
            .Select(x => new DbIndexDefinition(x)).ToArray();

    /// <summary>
    ///     Returns the quotes tableName.columnName combo
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="tableName"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static string GetQuotedColumn(this ISqlSyntaxProvider sql, string tableName, string columnName) =>
        sql.GetQuotedTableName(tableName) + "." + sql.GetQuotedColumnName(columnName);

    /// <summary>
    ///     This is used to generate a delete query that uses a sub-query to select the data, it is required because there's a
    ///     very particular syntax that
    ///     needs to be used to work for all servers
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    ///     See: http://issues.umbraco.org/issue/U4-3876
    /// </remarks>
    public static Sql GetDeleteSubquery(this ISqlSyntaxProvider sqlProvider, string tableName, string columnName, Sql subQuery, WhereInType whereInType = WhereInType.In) =>

        // TODO: This is no longer necessary since this used to be a specific requirement for MySql!
        // Now we can do a Delete<T> + sub query, see RelationRepository.DeleteByParent for example
        new Sql(
            string.Format(
            whereInType == WhereInType.In
                ? @"DELETE FROM {0} WHERE {1} IN (SELECT {1} FROM ({2}) x)"
                : @"DELETE FROM {0} WHERE {1} NOT IN (SELECT {1} FROM ({2}) x)",
            sqlProvider.GetQuotedTableName(tableName),
            sqlProvider.GetQuotedColumnName(columnName),
            subQuery.SQL), subQuery.Arguments);
}
