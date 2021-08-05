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
        protected void AddColumn<T>(string columnName) => AddColumn<T>(columnName, tableName: null, columns: null);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        protected void AddColumn<T>(string tableName, string columnName) => AddColumn<T>(columnName, tableName, columns: null);

        /// <summary>
        /// Adds the column (if it doesn't exist).
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="columnName">The name of the column.</param>
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName) => AddColumn<T>(columnName, tableName: null, columns);

        [Obsolete("Use AddColumn<T>(columns, columnName) instead, because that already checks whether the column does not exist.")]
        protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string columnName) => AddColumn<T>(columnName, tableName: null, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName) => AddColumn<T>(columnName, tableName, columns);

        [Obsolete("Use AddColumn<T>(columns, tableName, columnName) instead, because that already checks whether the column does not exist.")]
        protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName) => AddColumn<T>(columnName, tableName, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist).
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        private bool AddColumn<T>(string columnName, string tableName = null, IEnumerable<ColumnInfo> columns = null)
        {
            // TODO: Make this method protected and remove all other overloads
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);

            if (tableName == null) tableName = table.Name;
            if (columns == null) columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            if (ColumnExists(columns, tableName, columnName)) return false;

            var column = table.Columns.First(x => x.Name.InvariantEquals(columnName));
            var createSql = SqlSyntax.Format(column);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();

            return true;
        }

        /// <summary>
        /// Adds the column (if it doesn't exist) and sets an updated default value.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="updateAction">The update action.</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// All existing rows are fetched and loaded into memory and only the added column value is updated.
        /// </remarks>
        protected bool AddColumn<T>(string columnName, Func<T, T> updateAction, IEnumerable<ColumnInfo> columns = null)
        {
            var columnAdded = AddColumn<T>(columnName, out var sqls, null, columns);
            if (columnAdded)
            {
                foreach (var dto in Database.Fetch<T>()) Database.Update(updateAction(dto), new[] { columnName });
                foreach (var sql in sqls) Database.Execute(sql);
            }

            return columnAdded;
        }

        /// <summary>
        /// Adds the column (if it doesn't exist), but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        protected void AddColumn<T>(string columnName, out IEnumerable<string> sqls) => AddColumn<T>(columnName, out sqls, tableName: null, columns: null);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table, but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        protected void AddColumn<T>(string tableName, string columnName, out IEnumerable<string> sqls) => AddColumn<T>(columnName, out sqls, tableName, columns: null);

        /// <summary>
        /// Adds the column (if it doesn't exist), but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string columnName, out IEnumerable<string> sqls) => AddColumn<T>(columnName, out sqls, tableName: null, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table, but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName, out IEnumerable<string> sqls) => AddColumn<T>(columnName, out sqls, tableName, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table, but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        private bool AddColumn<T>(string columnName, out IEnumerable<string> sqls, string tableName = null, IEnumerable<ColumnInfo> columns = null)
        {
            // TODO: Make this method protected and remove all other overloads
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);

            if (tableName == null) tableName = table.Name;
            if (columns == null) columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            if (ColumnExists(columns, tableName, columnName))
            {
                sqls = Enumerable.Empty<string>();
                return false;
            }

            var column = table.Columns.First(x => x.Name.InvariantEquals(columnName));
            var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();

            return true;
        }
    }
}
