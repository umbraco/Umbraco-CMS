using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations
{
    /// <summary>
    /// Provides a base class to all migrations.
    /// </summary>
    /// <remarks>
    /// This file provides extra methods for migrations.
    /// </remarks>
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
        /// <returns>
        ///   <c>true</c> if the index exists; otherwise, <c>false</c>.
        /// </returns>
        protected bool ColumnExists(string tableName, string columnName)
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database);

            return ColumnExists(columns, tableName, columnName);
        }

        /// <summary>
        /// Determines whether the column exists in the specified table.
        /// </summary>
        /// <param name="columns">The existing column information to check the existance of the column against.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>
        ///   <c>true</c> if the index exists; otherwise, <c>false</c>.
        /// </returns>
        protected bool ColumnExists(IEnumerable<ColumnInfo> columns, string tableName, string columnName)
        {
            return columns.Any(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));
        }

        /// <summary>
        /// Gets the data type for the column in the specified table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>
        /// The data type for the column.
        /// </returns>
        protected string ColumnType(string tableName, string columnName)
        {
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            var column = columns.FirstOrDefault(x => x.TableName.InvariantEquals(tableName) && x.ColumnName.InvariantEquals(columnName));

            return column?.DataType;
        }
    }
}
