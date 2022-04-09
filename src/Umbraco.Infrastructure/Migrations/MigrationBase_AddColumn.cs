using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations
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
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        protected bool AddColumn<T>(string columnName, IEnumerable<ColumnInfo> columns = null, string tableName = null)
        {
            columns ??= SqlSyntax.GetColumnsInSchema(Context.Database);

            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            tableName ??= table.Name;

            if (ColumnExists(tableName, columnName, columns))
            {
                return false;
            }

            var column = GetColumnDefinition(table, columnName);
            var createSql = SqlSyntax.Format(column);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();

            return true;
        }

        /// <summary>
        /// Adds the column (if it doesn't exist) and executes an SQL query to update the default value.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="updateSql">The SQL query to update the default value.</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        protected bool AddColumn<T>(string columnName, NPoco.Sql updateSql, IEnumerable<ColumnInfo> columns = null, string tableName = null)
        {
            var columnAdded = AddColumn<T>(columnName, out var sqls, columns, tableName);
            if (columnAdded)
            {
                Database.Execute(updateSql);

                foreach (var sql in sqls)
                {
                    Database.Execute(sql);
                }
            }

            return columnAdded;
        }

        /// <summary>
        /// Adds the column (if it doesn't exist) and sets an updated default value for every existing row.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="updateAction">The action to set the updated default value (executed for every existing row).</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// All existing rows are fetched and loaded into memory and only the added column value is updated.
        /// </remarks>
        protected bool AddColumn<T>(string columnName, Func<T, T> updateAction, IEnumerable<ColumnInfo> columns = null, string tableName = null)
        {
            var columnAdded = AddColumn<T>(columnName, out var sqls, columns, tableName);
            if (columnAdded)
            {
                foreach (var dto in Database.Fetch<T>())
                {
                    Database.Update(updateAction(dto), new[] { columnName });
                }

                foreach (var sql in sqls)
                {
                    Database.Execute(sql);
                }
            }

            return columnAdded;
        }

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table, but always as nullable and returns the SQL queries to run after setting the default values on existing rows.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="sqls">The SQL queries to run after setting the default values on existing rows.</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        /// <returns>
        ///   <c>true</c> when the column didn't exist and was added; otherwise, <c>false</c>.
        /// </returns>
        protected bool AddColumn<T>(string columnName, out IEnumerable<string> sqls, IEnumerable<ColumnInfo> columns = null, string tableName = null)
        {
            columns ??= SqlSyntax.GetColumnsInSchema(Context.Database);

            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            tableName ??= table.Name;

            if (ColumnExists(tableName, columnName, columns))
            {
                sqls = Enumerable.Empty<string>();
                return false;
            }

            var column = GetColumnDefinition(table, columnName);
            var createSql = SqlSyntax.Format(column, SqlSyntax.GetQuotedTableName(tableName), out sqls);
            Execute.Sql(string.Format(SqlSyntax.AddColumn, SqlSyntax.GetQuotedTableName(tableName), createSql)).Do();

            return true;
        }
    }
}
