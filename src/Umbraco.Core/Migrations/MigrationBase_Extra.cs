using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

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
            var column = table.Columns.First(x => x.Name == columnName);
            var createSql = SqlSyntax.Format(column);
            Database.Execute(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(table.Name), createSql));
        }

        protected void AddColumn<T>(string tableName, string columnName)
        {
            //if (ColumnExists(tableName, columnName))
            //    throw new InvalidOperationException($"Column {tableName}.{columnName} already exists.");

            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            var column = table.Columns.First(x => x.Name == columnName);
            var createSql = SqlSyntax.Format(column);
            Database.Execute(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql));
        }

        protected void AddColumn<T>(string tableName, string columnName, out IEnumerable<string> sqls)
        {
            //if (ColumnExists(tableName, columnName))
            //    throw new InvalidOperationException($"Column {tableName}.{columnName} already exists.");

            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            var column = table.Columns.First(x => x.Name == columnName);
            var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
            Database.Execute(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql));
        }

        protected void AlterColumn<T>(string tableName, string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            var column = table.Columns.First(x => x.Name == columnName);
            SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out var sqls);
            foreach (var sql in sqls)
                Database.Execute(sql);
        }

        protected void ReplaceColumn<T>(string tableName, string currentName, string newName)
        {
            AddColumn<T>(tableName, newName, out var sqls);
            Database.Execute($"UPDATE {SqlSyntax.GetQuotedTableName(tableName)} SET {SqlSyntax.GetQuotedColumnName(newName)}={SqlSyntax.GetQuotedColumnName(currentName)}");
            foreach (var sql in sqls) Database.Execute(sql);
            Delete.Column(currentName).FromTable(tableName).Do();
        }

        protected bool TableExists(string tableName)
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            return tables.Any(x => x.InvariantEquals(tableName));
        }

        protected bool ColumnExists(string tableName, string columnName)
        {
            // that's ok even on MySql
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }

        protected string ColumnType(string tableName, string columnName)
        {
            // that's ok even on MySql
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).Distinct().ToArray();
            var column = columns.FirstOrDefault(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
            return column?.DataType;
        }
    }
}
