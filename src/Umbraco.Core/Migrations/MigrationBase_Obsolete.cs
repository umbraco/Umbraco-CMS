using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    /// <remarks>
    /// This file contains the obsoleted methods.
    /// </remarks>
    public abstract partial class MigrationBase
    {
        [Obsolete("Use AddColumn<T>(columns, columnName) instead, because that already checks whether the column does not exist.")]
        protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string columnName) => AddColumn<T>(columnName, null, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        [Obsolete("Use AddColumn<T>(columnName) instead, because the table name can be retrieved from the model type.")]
        protected void AddColumn<T>(string tableName, string columnName) => AddColumn<T>(columnName, tableName, null);

        [Obsolete("Use AddColumn<T>(columns, columnName) instead, because that already checks whether the column does not exist and the table name can be retrieved from the model type.")]
        protected void AddColumnIfNotExists<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName) => AddColumn<T>(columnName, tableName, columns);

        /// <summary>
        /// Adds the column (if it doesn't exist) to the specific table.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the column definition).</typeparam>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        [Obsolete("Use AddColumn<T>(columns, columnName) instead, because that already checks whether the column does not exist and the table name can be retrieved from the model type.")]
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName) => AddColumn<T>(columnName, tableName, columns);

        [Obsolete("Use AddColumn<T>(columnName, out sqls) instead, because the table name can be retrieved from the model type.")]
        protected void AddColumn<T>(string tableName, string columnName, out IEnumerable<string> sqls)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(table, tableName, columnName, out sqls);
        }

        [Obsolete("Use AddColumn<T>(columns, columnName, out sqls) instead, because the table name can be retrieved from the model type.")]
        protected void AddColumn<T>(IEnumerable<ColumnInfo> columns, string tableName, string columnName, out IEnumerable<string> sqls)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AddColumn(columns, table, tableName, columnName, out sqls);
        }

        [Obsolete("Use AlterColumn<T>(columnName) instead, because the table name can be retrieved from the model type.")]
        protected void AlterColumn<T>(string tableName, string columnName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            AlterColumn(table, tableName, columnName);
        }

        [Obsolete("Use ReplaceColumn<T>(currentName, newName) instead, because the table name can be retrieved from the model type.")]
        protected void ReplaceColumn<T>(string tableName, string currentName, string newName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            ReplaceColumn(table, tableName, currentName, newName);
        }
    }
}
