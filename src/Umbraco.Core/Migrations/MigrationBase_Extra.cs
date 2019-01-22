using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    public abstract partial class MigrationBase
    {
        // provides extra methods for migrations

        protected void AddColumn<T>(string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, table.Name, columnName);
        }

        protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            if (columns.Any(x => x.TableName.InvariantEquals(table.Name) && !x.ColumnName.InvariantEquals(columnName)))
                AddColumn(table, table.Name, columnName);
        }

        protected void AddColumn<T>(string tableName, string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, tableName, columnName);
        }

        protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            if (columns.Any(x => x.TableName.InvariantEquals(tableName) && !x.ColumnName.InvariantEquals(columnName)))
                AddColumn(table, tableName, columnName);
        }

        private void AddColumn(TableDefinition table, string tableName, string columnName)
        {
            if (ColumnExists(tableName, columnName)) return;

            var column = table.Columns.First(x => x.Name == columnName);
            var createSql = SqlSyntax.Format(column);
            
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
        }

        protected void AddColumn<T>(string columnName, out IEnumerable<string> sqls)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, table.Name, columnName, out sqls);
        }

        protected void AddColumn<T>(string tableName, string columnName, out IEnumerable<string> sqls)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, tableName, columnName, out sqls);
        }

        private void AddColumn(TableDefinition table, string tableName, string columnName, out IEnumerable<string> sqls)
        {
            if (ColumnExists(tableName, columnName))
            {
                sqls = Enumerable.Empty<string>();
                return;
            }

            var column = table.Columns.First(x => x.Name == columnName);
            var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
        }

        protected void AlterColumn<T>(string tableName, string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            var column = table.Columns.First(x => x.Name == columnName);
            SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out var sqls);
            foreach (var sql in sqls)
                Execute.Sql(sql).Do();
        }

        protected void ReplaceColumn<T>(string tableName, string currentName, string newName)
        {
            AddColumn<T>(tableName, newName, out var sqls);
            Execute.Sql($"UPDATE {SqlSyntax.GetQuotedTableName(tableName)} SET {SqlSyntax.GetQuotedColumnName(newName)}={SqlSyntax.GetQuotedColumnName(currentName)}").Do();
            foreach (var sql in sqls) Execute.Sql(sql).Do();
            Delete.Column(currentName).FromTable(tableName).Do();
        }

        protected bool TableExists(string tableName)
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            return tables.Any(x => x.InvariantEquals(tableName));
        }

        protected bool ColumnExists(string tableName, string columnName)
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }

        protected string ColumnType(string tableName, string columnName)
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            var column = columns.FirstOrDefault(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
            return column?.DataType;
        }
    }
}
