using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

/// <summary>
///     Defines an SqlSyntaxProvider
/// </summary>
public interface ISqlSyntaxProvider
{
    string ProviderName { get; }

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

    /// <summary>
    ///     Gets a regex matching aliased fields.
    /// </summary>
    /// <remarks>
    ///     <para>Matches "(table.column) AS (alias)" where table, column and alias are properly escaped.</para>
    /// </remarks>
    Regex AliasRegex { get; }

    string ConvertIntegerToOrderableString { get; }

    string ConvertDateToOrderableString { get; }

    string ConvertDecimalToOrderableString { get; }

    /// <summary>
    ///     Returns the default isolation level for the database
    /// </summary>
    IsolationLevel DefaultIsolationLevel { get; }

    string DbProvider { get; }

    IDictionary<Type, IScalarMapper>? ScalarMappers => null;

    DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString) =>
        current; // Default implementation.

    string EscapeString(string val);

    string GetWildcardPlaceholder();

    string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType);

    string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType);

    string GetConcat(params string[] args);

    string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string? referenceName = null, bool forInsert = false);

    string GetQuotedTableName(string? tableName);

    string GetQuotedColumnName(string? columnName);

    string GetQuotedName(string? name);

    bool DoesTableExist(IDatabase db, string tableName);

    string GetIndexType(IndexTypes indexTypes);

    string GetSpecialDbType(SpecialDbType dbType);

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

    string FormatColumnRename(string? tableName, string? oldName, string? newName);

    string FormatTableRename(string? oldName, string? newName);

    void HandleCreateTable(IDatabase database, TableDefinition tableDefinition, bool skipKeysAndIndexes = false);

    Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top);

    bool SupportsClustered();

    bool SupportsIdentityInsert();

    IEnumerable<string> GetTablesInSchema(IDatabase db);

    IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db);

    /// <summary>
    ///     Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not
    ///     include indexes)
    /// </summary>
    /// <param name="db"></param>
    /// <returns>
    ///     A Tuple containing: TableName, ConstraintName
    /// </returns>
    IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db);

    /// <summary>
    ///     Returns all constraints defined in the database (Primary keys, foreign keys, unique constraints...) (does not
    ///     include indexes)
    /// </summary>
    /// <param name="db"></param>
    /// <returns>
    ///     A Tuple containing: TableName, ColumnName, ConstraintName
    /// </returns>
    IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db);

    /// <summary>
    ///     Returns all defined Indexes in the database excluding primary keys
    /// </summary>
    /// <param name="db"></param>
    /// <returns>
    ///     A Tuple containing: TableName, IndexName, ColumnName, IsUnique
    /// </returns>
    IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db);

    /// <summary>
    ///     Tries to gets the name of the default constraint on a column.
    /// </summary>
    /// <param name="db">The database.</param>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <param name="constraintName">The constraint name.</param>
    /// <returns>A value indicating whether a default constraint was found.</returns>
    /// <remarks>
    ///     <para>
    ///         Some database engines may not have names for default constraints,
    ///         in which case the function may return true, but <paramref name="constraintName" /> is
    ///         unspecified.
    ///     </para>
    /// </remarks>
    bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName);

    string GetFieldNameForUpdate<TDto>(Expression<Func<TDto, object?>> fieldSelector, string? tableAlias = null);

    /// <summary>
    ///     Appends the relevant ForUpdate hint.
    /// </summary>
    Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql);

    /// <summary>
    ///     Appends the relevant ForUpdate hint.
    /// </summary>
    Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql);

    /// <summary>
    ///     Handles left join with nested join
    /// </summary>
    Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
        Sql<ISqlContext> sql,
        Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
        string? alias = null);
}
