using System;
using System.Collections.Generic;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    internal static class SqlSyntaxProviderExtensions
    {
        /// <summary>
        /// Returns the quotes tableName.columnName combo
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tableName"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static string GetQuotedColumn(this ISqlSyntaxProvider sql, string tableName, string columnName)
        {
            return sql.GetQuotedTableName(tableName) + "." + sql.GetQuotedColumnName(columnName);
        }
    }

    /// <summary>
    /// Defines an SqlSyntaxProvider
    /// </summary>
    public interface ISqlSyntaxProvider
    {
        string GetStringColumnEqualComparison(string column, string value, TextColumnType columnType);
        string GetStringColumnStartsWithComparison(string column, string value, TextColumnType columnType);
        string GetStringColumnEndsWithComparison(string column, string value, TextColumnType columnType);
        string GetStringColumnContainsComparison(string column, string value, TextColumnType columnType);
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
        string CreateForeignKeyConstraint { get; }
        string DeleteDefaultConstraint { get; }
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
        bool SupportsClustered();
        bool SupportsIdentityInsert();
        bool? SupportsCaseInsensitiveQueries(Database db);
        IEnumerable<string> GetTablesInSchema(Database db);
        IEnumerable<ColumnInfo> GetColumnsInSchema(Database db);
        IEnumerable<Tuple<string, string>> GetConstraintsPerTable(Database db);
        IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(Database db);
    }
}