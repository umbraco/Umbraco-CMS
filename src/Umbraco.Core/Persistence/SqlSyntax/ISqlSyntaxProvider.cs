using System;
using System.Collections.Generic;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Defines an SqlSyntaxProvider
    /// </summary>
    public interface ISqlSyntaxProvider
    {
        string EscapeString(string val);

        string GetWildcardPlaceholder();
        string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType);
        string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType);

        [Obsolete("Use the overload with the parameter index instead")]
        string GetStringColumnEqualComparison(string column, string value, TextColumnType columnType);
        [Obsolete("Use the overload with the parameter index instead")]
        string GetStringColumnStartsWithComparison(string column, string value, TextColumnType columnType);
        [Obsolete("Use the overload with the parameter index instead")]
        string GetStringColumnEndsWithComparison(string column, string value, TextColumnType columnType);
        [Obsolete("Use the overload with the parameter index instead")]
        string GetStringColumnContainsComparison(string column, string value, TextColumnType columnType);
        [Obsolete("Use the overload with the parameter index instead")]
        string GetStringColumnWildcardComparison(string column, string value, TextColumnType columnType);

        string GetQuotedTableName(string tableName);
        string GetQuotedColumnName(string columnName);
        string GetQuotedName(string name);
        bool DoesTableExist(Database db, string tableName);
        string GetIndexType(IndexTypes indexTypes);
        string GetSpecialDbType(SpecialDbTypes dbTypes);
        string CreateTable { get; }
        string DropTable { get; }
        string AddColumn { get; }
        string DropColumn { get; }
        string AlterColumn { get; }
        string RenameColumn { get; }
        string RenameTable { get; }
        string CreateSchema { get; }
        string AlterSchema { get; }
        string DropSchema { get; }
        string CreateIndex { get; }
        string DropIndex { get; }
        string InsertData { get; }
        string UpdateData { get; }
        string DeleteData { get; }
        string TruncateTable { get; }
        string CreateConstraint { get; }
        string DeleteConstraint { get; }

        [Obsolete("This is never used, use the Format(ForeignKeyDefinition) instead")]
        string CreateForeignKeyConstraint { get; }
        string DeleteDefaultConstraint { get; }
        string FormatDateTime(DateTime date, bool includeTime = true);
        string Format(TableDefinition table);
        string Format(IEnumerable<ColumnDefinition> columns);
        List<string> Format(IEnumerable<IndexDefinition> indexes);
        List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys);
        string FormatPrimaryKey(TableDefinition table);
        string GetQuotedValue(string value);
        string Format(ColumnDefinition column);
        string Format(IndexDefinition index);
        string Format(ForeignKeyDefinition foreignKey);
        string FormatColumnRename(string tableName, string oldName, string newName);
        string FormatTableRename(string oldName, string newName);
        Sql SelectTop(Sql sql, int top);
        bool SupportsClustered();
        bool SupportsIdentityInsert();
        bool? SupportsCaseInsensitiveQueries(Database db);
        
        string ConvertIntegerToOrderableString { get; }
        string ConvertDateToOrderableString { get; }
        string ConvertDecimalToOrderableString { get; }

        IEnumerable<string> GetTablesInSchema(Database db);
        IEnumerable<ColumnInfo> GetColumnsInSchema(Database db);

        /// <summary>
        /// Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not include indexes)
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A Tuple containing: TableName, ConstraintName
        /// </returns>
        IEnumerable<Tuple<string, string>> GetConstraintsPerTable(Database db);

        /// <summary>
        /// Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not include indexes)
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A Tuple containing: TableName, ColumnName, ConstraintName
        /// </returns>
        IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(Database db);

        /// <summary>
        /// Returns all defined Indexes in the database excluding primary keys
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A Tuple containing: TableName, IndexName, ColumnName, IsUnique
        /// </returns>
        IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(Database db);
    }
}
