using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    /// <remarks>
    /// This file provides extra ReplaceColumn methods for migrations.
    /// </remarks>
    public abstract partial class MigrationBase
    {
        /// <summary>
        /// Replaces the column.
        /// </summary>
        /// <typeparam name="T">The model type to get the table definition from (used for the table name and column definition).</typeparam>
        /// <param name="currentName">The current name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        protected void ReplaceColumn<T>(string currentName, string newName) => ReplaceColumn<T>(null, currentName, newName);

        /// <summary>
        /// Replaces the column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="currentName">The current name of the column.</param>
        /// <param name="newName">The new name of the column.</param>
        protected void ReplaceColumn<T>(string tableName, string currentName, string newName)
        {
            // TODO: Make tableName optional with a default null value
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);

            if (tableName == null) tableName = table.Name;

            if (DatabaseType.IsSqlCe())
            {
                if (AddColumn<T>(newName, out var sqls, tableName))
                {
                    Execute.Sql($"UPDATE {SqlSyntax.GetQuotedTableName(tableName)} SET {SqlSyntax.GetQuotedColumnName(newName)}={SqlSyntax.GetQuotedColumnName(currentName)}").Do();
                    foreach (var sql in sqls) Execute.Sql(sql).Do();
                    Delete.Column(currentName).FromTable(tableName).Do();
                }
            }
            else
            {
                Execute.Sql(SqlSyntax.FormatColumnRename(tableName, currentName, newName)).Do();
                AlterColumn<T>(tableName, newName);
            }
        }
    }
}
