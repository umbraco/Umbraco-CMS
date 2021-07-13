using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations
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
        /// Alters the column.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">Name of the column.</param>
        protected void AlterColumn<T>(string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AlterColumn(table, table.Name, columnName);
        }

        /// <summary>
        /// Alters the column of the specific table.
        /// </summary>
        /// <param name="table">The table definition.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        private void AlterColumn(TableDefinition table, string tableName, string columnName)
        {
            var column = table.Columns.First(x => x.Name.InvariantEquals(columnName));
            SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out var sqls);
            foreach (var sql in sqls) Execute.Sql(sql).Do();
        }
    }
}
