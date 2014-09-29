namespace Umbraco.Core.Persistence.SqlSyntax
{
    internal static class SqlSyntaxProviderExtensions
    {
        /// <summary>
        /// Returns the quotes tableName.columnName combo
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetQuotedColumn(this ISqlSyntaxProvider sql, string tableName, string columnName)
        {
            return sql.GetQuotedTableName(tableName) + "." + sql.GetQuotedColumnName(columnName);
        }

        /// <summary>
        /// This is used to generate a delete query that uses a sub-query to select the data, it is required because there's a very particular syntax that
        /// needs to be used to work for all servers: MySql, SQLCE and MSSQL
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// See: http://issues.umbraco.org/issue/U4-3876
        /// </remarks>
        public static Sql GetDeleteSubquery(this ISqlSyntaxProvider sqlProvider, string tableName, string columnName, Sql subQuery)
        {
            return new Sql(string.Format(@"DELETE FROM {0} WHERE {1} IN (SELECT {1} FROM ({2}) x)",
                sqlProvider.GetQuotedTableName(tableName),
                sqlProvider.GetQuotedColumnName(columnName),
                subQuery.SQL), subQuery.Arguments);
        }
    }
}