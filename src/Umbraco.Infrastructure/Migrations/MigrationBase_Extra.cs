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
    public abstract partial class MigrationBase
    {
        /// <summary>
        /// Determines whether the table exists.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>
        ///   <c>true</c> if the table exists; otherwise, <c>false</c>.
        /// </returns>
        protected bool TableExists(string tableName)
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);

            return tables.Any(x => x.InvariantEquals(tableName));
        }

        /// <summary>
        /// Determines whether the index exists.
        /// </summary>
        /// <param name="indexName">The name of the index.</param>
        /// <returns>
        ///   <c>true</c> if the index exists; otherwise, <c>false</c>.
        /// </returns>
        protected bool IndexExists(string indexName)
        {
            var indexes = SqlSyntax.GetDefinedIndexes(Context.Database);

            return indexes.Any(x => x.Item2.InvariantEquals(indexName));
        }

        /// <summary>
        /// Determines whether the column exists in the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <returns>
        ///   <c>true</c> if the index exists; otherwise, <c>false</c>.
        /// </returns>
        protected bool ColumnExists(string tableName, string columnName, IEnumerable<ColumnInfo> columns = null)
        {
            columns ??= SqlSyntax.GetColumnsInSchema(Context.Database);

            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }

        /// <summary>
        /// Gets the data type for the column in the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <param name="columns">The existing column information to check the existance of the column against (if <c>null</c>, gets the current columns).</param>
        /// <returns>
        /// The data type for the column.
        /// </returns>
        protected string GetColumnDataType(string tableName, string columnName, IEnumerable<ColumnInfo> columns = null)
        {
            columns ??= SqlSyntax.GetColumnsInSchema(Context.Database);

            var column = columns.FirstOrDefault(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));

            return column?.DataType;
        }

        private static ColumnDefinition GetColumnDefinition(TableDefinition table, string columnName)
        {
            var column = table.Columns.FirstOrDefault(x => x.Name.InvariantEquals(columnName));
            if (column is null)
            {
                throw new InvalidOperationException($"Could not find column '{columnName}' in table definition for '{table.Name}'.");
            }

            return column;
        }
    }
}
