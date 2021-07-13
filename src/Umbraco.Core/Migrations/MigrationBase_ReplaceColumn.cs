using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence;
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
        /// Replaces the column.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="currentName">The current name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        protected void ReplaceColumn<T>(string currentName, string newName)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            ReplaceColumn(table, table.Name, currentName, newName);
        }

        /// <summary>
        /// Replaces the column.
        /// </summary>
        /// <param name="table">The table definition.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="currentName">The current name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        private void ReplaceColumn(TableDefinition table, string tableName, string currentName, string newName)
        {
            if (DatabaseType.IsSqlCe())
            {
                AddColumn(table, tableName, newName, out var sqls);
                Execute.Sql($"UPDATE {SqlSyntax.GetQuotedTableName(tableName)} SET {SqlSyntax.GetQuotedColumnName(newName)}={SqlSyntax.GetQuotedColumnName(currentName)}").Do();
                foreach (var sql in sqls) Execute.Sql(sql).Do();
                Delete.Column(currentName).FromTable(tableName).Do();
            }
            else
            {
                Execute.Sql(SqlSyntax.FormatColumnRename(tableName, currentName, newName)).Do();
                AlterColumn(table, tableName, newName);
            }
        }
    }
}
