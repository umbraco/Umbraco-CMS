using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    /// <remarks>
    /// This file provides extra AddColumn methods for migrations.
    /// </remarks>
    public abstract partial class MigrationBase
    {
        /// <summary>
        /// Adds the column (if it doesn't exist).
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        protected void AddColumn<T>(string columnName) => AddColumn<T>(columnName, null, null);

        /// <summary>
        /// Adds the column (if it doesn't exist).
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="columnName">The name of the column.</param>
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName) => AddColumn<T>(columnName, null, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table.
        /// </summary>
        /// <param name="table">The table definition.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        private void AddColumn<T>(string columnName, string tableName = null, IEnumerable<ColumnInfo> columns = null)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);

            if (tableName == null) tableName = table.Name;
            if (columns == null) columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            if (ColumnExists(columns, tableName, columnName)) return;

            var column = table.Columns.First(x => x.Name.InvariantEquals(columnName));
            var createSql = SqlSyntax.Format(column);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
        }

        protected void AddColumn<T>(string columnName, Func<T, T> updateAction)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, table.Name, columnName, updateAction);
        }

        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName, Func<T, T> updateAction)
        { }

        private void AddColumn<T>(TableDefinition table, string tableName, string columnName, Func<T, T> updateAction)
        {
            if (ColumnExists(tableName, columnName)) return;

            var column = table.Columns.First(x => x.Name == columnName);
            var createSql = SqlSyntax.Format(column);

            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            AddColumn(columns, table, tableName, columnName, updateAction);
        }

        //private void AddColumn(IEnumerable<ColumnInfo> columns, TableDefinition table, string tableName, string columnName, object defaultValue)
        //{
        //    AddColumn(columns, table, tableName, columnName, out var sqls);

        //    Database.Execute($@"UPDATE {SqlSyntax.GetQuotedTableName(tableName)} SET {SqlSyntax.GetQuotedColumnName(columnName)} = {0}", defaultValue);

        //    foreach (var sql in sqls)
        //    {
        //        Database.Execute(sql);
        //    }
        //}

        private void AddColumn<T>(IEnumerable<ColumnInfo> columns, TableDefinition table, string tableName, string columnName, Func<T, T> updateAction)
        {
            AddColumn(columns, table, tableName, columnName, out var sqls);
            foreach (var dto in Database.Fetch<T>()) Database.Update(updateAction(dto), new[] { columnName });
            foreach (var sql in sqls) Database.Execute(sql);
        }

        /// <summary>
        /// Adds the column (if it doesn't exist), but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        protected void AddColumn<T>(string columnName, out IEnumerable<string> sqls)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, table.Name, columnName, out sqls);
        }

        /// <summary>
        /// Adds the column (if it doesn't exist), but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName, out IEnumerable<string> sqls)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(columns, table, table.Name, columnName, out sqls);
        }

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table, but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <param name="table">The table definition.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        private void AddColumn(TableDefinition table, string tableName, string columnName, out IEnumerable<string> sqls)
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            AddColumn(columns, table, tableName, columnName, out sqls);
        }

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table, but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="table">The table definition.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        private void AddColumn(IEnumerable<ColumnInfo> columns, TableDefinition table, string tableName, string columnName, out IEnumerable<string> sqls)
        {
            if (ColumnExists(columns, tableName, columnName))
            {
                sqls = Enumerable.Empty<string>();
                return;
            }

            var column = table.Columns.First(x => x.Name.InvariantEquals(columnName));
            var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();
        }
    }
}
