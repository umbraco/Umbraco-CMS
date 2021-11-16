using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    public interface ISqlSyntaxProvider2 : ISqlSyntaxProvider
    {
        void ReadLock(IDatabase db, TimeSpan timeout, int lockId);
        void WriteLock(IDatabase db, TimeSpan timeout, int lockId);
    }

    /// <summary>
    /// Defines an SqlSyntaxProvider
    /// </summary>
    public interface ISqlSyntaxProvider
    {
        string EscapeString(string val);

        string GetWildcardPlaceholder();
        string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType);
        string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType);
        string GetConcat(params string[] args);

        string GetQuotedTableName(string tableName);
        string GetQuotedColumnName(string columnName);
        string GetQuotedName(string name);
        bool DoesTableExist(IDatabase db, string tableName);
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

        string DeleteDefaultConstraint { get; }
        string FormatDateTime(DateTime date, bool includeTime = true);
        string Format(TableDefinition table);
        string Format(IEnumerable<ColumnDefinition> columns);
        List<string> Format(IEnumerable<IndexDefinition> indexes);
        List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys);
        string FormatPrimaryKey(TableDefinition table);
        string GetQuotedValue(string value);
        string Format(ColumnDefinition column);
        string Format(ColumnDefinition column, string tableName, out IEnumerable<string> sqls);
        string Format(IndexDefinition index);
        string Format(ForeignKeyDefinition foreignKey);
        string FormatColumnRename(string tableName, string oldName, string newName);
        string FormatTableRename(string oldName, string newName);

        /// <summary>
        /// Gets a regex matching aliased fields.
        /// </summary>
        /// <remarks>
        /// <para>Matches "(table.column) AS (alias)" where table, column and alias are properly escaped.</para>
        /// </remarks>
        Regex AliasRegex { get; }

        Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top);

        bool SupportsClustered();
        bool SupportsIdentityInsert();

        string ConvertIntegerToOrderableString { get; }
        string ConvertDateToOrderableString { get; }
        string ConvertDecimalToOrderableString { get; }

        /// <summary>
        /// Returns the default isolation level for the database
        /// </summary>
        IsolationLevel DefaultIsolationLevel { get; }

        IEnumerable<string> GetTablesInSchema(IDatabase db);
        IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db);

        /// <summary>
        /// Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not include indexes)
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A Tuple containing: TableName, ConstraintName
        /// </returns>
        IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db);

        /// <summary>
        /// Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not include indexes)
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A Tuple containing: TableName, ColumnName, ConstraintName
        /// </returns>
        IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db);

        /// <summary>
        /// Returns all defined Indexes in the database excluding primary keys
        /// </summary>
        /// <param name="db"></param>
        /// <returns>
        /// A Tuple containing: TableName, IndexName, ColumnName, IsUnique
        /// </returns>
        IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db);

        /// <summary>
        /// Tries to gets the name of the default constraint on a column.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="columnName">The column name.</param>
        /// <param name="constraintName">The constraint name.</param>
        /// <returns>A value indicating whether a default constraint was found.</returns>
        /// <remarks>
        /// <para>Some database engines (e.g. SqlCe) may not have names for default constraints,
        /// in which case the function may return true, but <paramref name="constraintName"/> is
        /// unspecified.</para>
        /// </remarks>
        bool TryGetDefaultConstraint(IDatabase db, string tableName, string columnName, out string constraintName);

        void ReadLock(IDatabase db, params int[] lockIds);
        void WriteLock(IDatabase db, params int[] lockIds);
    }
}
