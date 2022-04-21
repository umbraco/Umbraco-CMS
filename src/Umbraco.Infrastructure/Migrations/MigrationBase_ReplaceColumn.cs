using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations
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
        /// <param name="tableName">The name of the table (if <c>null</c>, uses the name from the table definition).</param>
        protected void ReplaceColumn<T>(string currentName, string newName, string tableName = null)
        {
            var table = DefinitionFactory.GetTableDefinition(typeof(T), SqlSyntax);
            tableName ??= table.Name;

            Execute.Sql(SqlSyntax.FormatColumnRename(tableName, currentName, newName)).Do();
            AlterColumn<T>(newName, tableName);
        }
    }
}
