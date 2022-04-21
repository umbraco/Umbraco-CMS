using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    /// <remarks>
    /// This file provides extra AlterColumn methods for migrations.
    /// </remarks>
    public abstract partial class MigrationBase
    {
        /// <summary>
        /// Alters the column of the specific table.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        protected void AlterColumn<T>(string columnName, string tableName = null)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            tableName ??= table.Name;

            var column = GetColumnDefinition(table, columnName);
            SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out var sqls);
            foreach (var sql in sqls)
            {
                Execute.Sql(sql).Do();
            }
        }
    }
}
