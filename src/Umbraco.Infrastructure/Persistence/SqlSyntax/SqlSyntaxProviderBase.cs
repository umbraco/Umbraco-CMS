// Don't remove the unused System using, for some reason this breaks docfx, and I have no clue why.
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

/// <summary>
///     Represents the Base Sql Syntax provider implementation.
/// </summary>
/// <remarks>
///     All Sql Syntax provider implementations should derive from this abstract class.
/// </remarks>
/// <typeparam name="TSyntax"></typeparam>
public abstract class SqlSyntaxProviderBase<TSyntax> : ISqlSyntaxProvider
    where TSyntax : ISqlSyntaxProvider
{
    private readonly Lazy<DbTypes> _dbTypes;

    protected SqlSyntaxProviderBase()
    {
        ClauseOrder = new List<Func<ColumnDefinition, string>>
        {
            FormatString,
            FormatType,
            FormatNullable,
            FormatConstraint,
            FormatDefaultValue,
            FormatPrimaryKey,
            FormatIdentity
        };

        //defaults for all providers
        StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
        StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);
        DecimalColumnDefinition =
            string.Format(DecimalColumnDefinitionFormat, DefaultDecimalPrecision, DefaultDecimalScale);

        // ReSharper disable VirtualMemberCallInConstructor
        // ok to call virtual GetQuotedXxxName here - they don't depend on any state
        var col = Regex.Escape(GetQuotedColumnName("column")).Replace("column", @"\w+");
        var fld = Regex.Escape(GetQuotedTableName("table") + ".").Replace("table", @"\w+") + col;
        // ReSharper restore VirtualMemberCallInConstructor
        AliasRegex = new Regex(
            "(" + fld + @")\s+AS\s+(" + col + ")",
            RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        _dbTypes = new Lazy<DbTypes>(InitColumnTypeMap);
    }

    public string StringLengthNonUnicodeColumnDefinitionFormat { get; } = "VARCHAR({0})";

    public virtual string StringLengthUnicodeColumnDefinitionFormat { get; } = "NVARCHAR({0})";

    public string DecimalColumnDefinitionFormat { get; } = "DECIMAL({0},{1})";

    public string DefaultValueFormat { get; } = "DEFAULT ({0})";

    public int DefaultStringLength { get; } = 255;

    public int DefaultDecimalPrecision { get; } = 20;

    public int DefaultDecimalScale { get; } = 9;

    //Set by Constructor
    public virtual string StringColumnDefinition { get; }

    public string StringLengthColumnDefinitionFormat { get; }

    public string AutoIncrementDefinition { get; protected set; } = "AUTOINCREMENT";

    public string IntColumnDefinition { get; protected set; } = "INTEGER";

    public string LongColumnDefinition { get; protected set; } = "BIGINT";

    public string GuidColumnDefinition { get; protected set; } = "GUID";

    public string BoolColumnDefinition { get; protected set; } = "BOOL";

    public string RealColumnDefinition { get; protected set; } = "DOUBLE";

    public string DecimalColumnDefinition { get; protected set; }

    public string BlobColumnDefinition { get; protected set; } = "BLOB";

    public string DateTimeColumnDefinition { get; protected set; } = "DATETIME";

    public string DateTimeOffsetColumnDefinition { get; protected set; } = "DATETIMEOFFSET(7)";

    public string TimeColumnDefinition { get; protected set; } = "DATETIME";

    protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; }

    protected DbTypes DbTypeMap => _dbTypes.Value;

    public virtual string CreateForeignKeyConstraint =>
        "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}";

    public virtual string CreateDefaultConstraint => "ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT ({2}) FOR {3}";

    public Regex AliasRegex { get; }

    public string GetWildcardPlaceholder() => "%";

    public virtual DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString) => current;

    public abstract string ProviderName { get; }

    public virtual string EscapeString(string val) => NPocoDatabaseExtensions.EscapeAtSymbols(val.Replace("'", "''"));

    public virtual string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType) =>
        //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
        $"upper({column}) = upper(@{paramIndex})";

    public virtual string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType) =>
        //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
        $"upper({column}) LIKE upper(@{paramIndex})";

    public virtual string GetConcat(params string[] args) => "concat(" + string.Join(",", args) + ")";

    public virtual string GetQuotedTableName(string? tableName) => $"\"{tableName}\"";

    public virtual string GetQuotedColumnName(string? columnName) => $"\"{columnName}\"";

    public virtual string GetQuotedName(string? name) => $"\"{name}\"";

    public virtual string GetQuotedValue(string value) => $"'{value}'";

    public virtual string GetIndexType(IndexTypes indexTypes)
    {
        string indexType;

        if (indexTypes == IndexTypes.Clustered)
        {
            indexType = "CLUSTERED";
        }
        else
        {
            indexType = indexTypes == IndexTypes.NonClustered
                ? "NONCLUSTERED"
                : "UNIQUE NONCLUSTERED";
        }

        return indexType;
    }

    public virtual string GetSpecialDbType(SpecialDbType dbType)
    {
        if (dbType == SpecialDbType.NCHAR)
        {
            return SpecialDbType.NCHAR;
        }

        if (dbType == SpecialDbType.NTEXT)
        {
            return SpecialDbType.NTEXT;
        }

        if (dbType == SpecialDbType.NVARCHARMAX)
        {
            return "NVARCHAR(MAX)";
        }

        return "NVARCHAR";
    }

    public virtual string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string? referenceName = null, bool forInsert = false)
    {
        tableName = GetQuotedTableName(tableName);
        columnName = GetQuotedColumnName(columnName);
        var column = tableName + "." + columnName;
        if (columnAlias == null)
        {
            return column;
        }

        referenceName = referenceName == null ? string.Empty : referenceName + "__";
        columnAlias = GetQuotedColumnName(referenceName + columnAlias);
        column += " AS " + columnAlias;
        return column;
    }


    public abstract IsolationLevel DefaultIsolationLevel { get; }

    public abstract string DbProvider { get; }

    public virtual IEnumerable<string> GetTablesInSchema(IDatabase db) => new List<string>();

    public virtual IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db) => new List<ColumnInfo>();

    public virtual IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db) =>
        new List<Tuple<string, string>>();

    public virtual IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db) =>
        new List<Tuple<string, string, string>>();

    public abstract IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db);

    public abstract bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName);

    public virtual string GetFieldNameForUpdate<TDto>(
        Expression<Func<TDto, object?>> fieldSelector,
        string? tableAlias = null) => this.GetFieldName(fieldSelector, tableAlias);

    public virtual Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql) => sql;

    public virtual Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql) => sql;

    public abstract Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
        Sql<ISqlContext> sql,
        Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
        string? alias = null);

    public virtual IDictionary<Type, IScalarMapper>? ScalarMappers => null;

    public virtual bool DoesTableExist(IDatabase db, string tableName) => GetTablesInSchema(db).Contains(tableName);

    public virtual bool SupportsClustered() => true;

    public virtual bool SupportsIdentityInsert() => true;

    /// <summary>
    ///     This is used ONLY if we need to format datetime without using SQL parameters (i.e. during migrations)
    /// </summary>
    /// <param name="date"></param>
    /// <param name="includeTime"></param>
    /// <returns></returns>
    /// <remarks>
    ///     MSSQL has a DateTime standard that is unambiguous and works on all servers:
    ///     YYYYMMDD HH:mm:ss
    /// </remarks>
    public virtual string FormatDateTime(DateTime date, bool includeTime = true) =>
        // need CultureInfo.InvariantCulture because ":" here is the "time separator" and
        // may be converted to something else in different cultures (eg "." in DK).
        date.ToString(includeTime ? "yyyyMMdd HH:mm:ss" : "yyyyMMdd", CultureInfo.InvariantCulture);

    public virtual string Format(TableDefinition table)
    {
        var statement = string.Format(CreateTable, GetQuotedTableName(table.Name), Format(table.Columns));

        return statement;
    }

    public virtual List<string> Format(IEnumerable<IndexDefinition> indexes) => indexes.Select(Format).ToList();

    public virtual string Format(IndexDefinition index)
    {
        var name = string.IsNullOrEmpty(index.Name)
            ? $"IX_{index.TableName}_{index.ColumnName}"
            : index.Name;

        var columns = index.Columns.Any()
            ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
            : GetQuotedColumnName(index.ColumnName);

        return string.Format(
            CreateIndex,
            GetIndexType(index.IndexType),
            " ",
            GetQuotedName(name),
            GetQuotedTableName(index.TableName),
            columns);
    }

    public virtual List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys) =>
        foreignKeys.Select(Format).ToList();

    public virtual string Format(ForeignKeyDefinition foreignKey)
    {
        var constraintName = string.IsNullOrEmpty(foreignKey.Name)
            ? $"FK_{foreignKey.ForeignTable}_{foreignKey.PrimaryTable}_{foreignKey.PrimaryColumns.First()}"
            : foreignKey.Name;

        return string.Format(
            CreateForeignKeyConstraint,
            GetQuotedTableName(foreignKey.ForeignTable),
            GetQuotedName(constraintName),
            GetQuotedColumnName(foreignKey.ForeignColumns.First()),
            GetQuotedTableName(foreignKey.PrimaryTable),
            GetQuotedColumnName(foreignKey.PrimaryColumns.First()),
            FormatCascade("DELETE", foreignKey.OnDelete),
            FormatCascade("UPDATE", foreignKey.OnUpdate));
    }

    public virtual string Format(IEnumerable<ColumnDefinition> columns)
    {
        var sb = new StringBuilder();
        foreach (ColumnDefinition column in columns)
        {
            sb.Append(Format(column) + ",\n");
        }

        return sb.ToString().TrimEnd(",\n");
    }

    public virtual string Format(ColumnDefinition column) =>
        string.Join(" ", ClauseOrder
            .Select(action => action(column))
            .Where(clause => string.IsNullOrEmpty(clause) == false));

    public virtual string Format(ColumnDefinition column, string tableName, out IEnumerable<string> sqls)
    {
        var sql = new StringBuilder();
        sql.Append(FormatString(column));
        sql.Append(" ");
        sql.Append(FormatType(column));
        sql.Append(" ");
        sql.Append("NULL"); // always nullable
        sql.Append(" ");
        sql.Append(FormatConstraint(column));
        sql.Append(" ");
        sql.Append(FormatDefaultValue(column));
        sql.Append(" ");
        sql.Append(FormatPrimaryKey(column));
        sql.Append(" ");
        sql.Append(FormatIdentity(column));

        //var isNullable = column.IsNullable;

        //var constraint = FormatConstraint(column)?.TrimStart("CONSTRAINT ");
        //var hasConstraint = !string.IsNullOrWhiteSpace(constraint);

        //var defaultValue = FormatDefaultValue(column);
        //var hasDefaultValue = !string.IsNullOrWhiteSpace(defaultValue);

        // TODO: This used to exit if nullable but that means this would never work
        // to return SQL if the column was nullable?!? I don't get it. This was here
        // 4 years ago, I've removed it so that this works for nullable columns.
        //if (isNullable /*&& !hasConstraint && !hasDefaultValue*/)
        //{
        //    sqls = Enumerable.Empty<string>();
        //    return sql.ToString();
        //}

        var msql = new List<string>();
        sqls = msql;

        var alterSql = new StringBuilder();
        alterSql.Append(FormatString(column));
        alterSql.Append(" ");
        alterSql.Append(FormatType(column));
        alterSql.Append(" ");
        alterSql.Append(FormatNullable(column));
        //alterSql.Append(" ");
        //alterSql.Append(FormatPrimaryKey(column));
        //alterSql.Append(" ");
        //alterSql.Append(FormatIdentity(column));
        msql.Add(string.Format(AlterColumn, tableName, alterSql));

        //if (hasConstraint)
        //{
        //    var dropConstraintSql = string.Format(DeleteConstraint, tableName, constraint);
        //    msql.Add(dropConstraintSql);
        //    var constraintType = hasDefaultValue ? defaultValue : "";
        //    var createConstraintSql = string.Format(CreateConstraint, tableName, constraint, constraintType, FormatString(column));
        //    msql.Add(createConstraintSql);
        //}

        return sql.ToString();
    }

    public virtual string FormatPrimaryKey(TableDefinition table)
    {
        ColumnDefinition? columnDefinition = table.Columns.FirstOrDefault(x => x.IsPrimaryKey);
        if (columnDefinition == null)
        {
            return string.Empty;
        }

        var constraintName = string.IsNullOrEmpty(columnDefinition.PrimaryKeyName)
            ? $"PK_{table.Name}"
            : columnDefinition.PrimaryKeyName;

        var columns = string.IsNullOrEmpty(columnDefinition.PrimaryKeyColumns)
            ? GetQuotedColumnName(columnDefinition.Name)
            : string.Join(", ", columnDefinition.PrimaryKeyColumns
                .Split(Constants.CharArrays.CommaSpace, StringSplitOptions.RemoveEmptyEntries)
                .Select(GetQuotedColumnName));

        var primaryKeyPart =
            string.Concat("PRIMARY KEY", columnDefinition.IsIndexed ? " CLUSTERED" : " NONCLUSTERED");

        return string.Format(
            CreateConstraint,
            GetQuotedTableName(table.Name),
            GetQuotedName(constraintName),
            primaryKeyPart,
            columns);
    }

    public virtual string FormatColumnRename(string? tableName, string? oldName, string? newName) =>
        string.Format(
            RenameColumn,
            GetQuotedTableName(tableName),
            GetQuotedColumnName(oldName),
            GetQuotedColumnName(newName));

    public virtual string FormatTableRename(string? oldName, string? newName) =>
        string.Format(RenameTable, GetQuotedTableName(oldName), GetQuotedTableName(newName));

    public abstract Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top);

    public abstract void HandleCreateTable(
        IDatabase database,
        TableDefinition tableDefinition,
        bool skipKeysAndIndexes = false);

    public virtual string DeleteDefaultConstraint =>
        throw new NotSupportedException("Default constraints are not supported");

    public virtual string CreateTable => "CREATE TABLE {0} ({1})";

    public virtual string DropTable => "DROP TABLE {0}";

    public virtual string AddColumn => "ALTER TABLE {0} ADD {1}";

    public virtual string DropColumn => "ALTER TABLE {0} DROP COLUMN {1}";

    public virtual string AlterColumn => "ALTER TABLE {0} ALTER COLUMN {1}";

    public virtual string RenameColumn => "ALTER TABLE {0} RENAME COLUMN {1} TO {2}";

    public virtual string RenameTable => "RENAME TABLE {0} TO {1}";

    public virtual string CreateSchema => "CREATE SCHEMA {0}";

    public virtual string AlterSchema => "ALTER SCHEMA {0} TRANSFER {1}.{2}";

    public virtual string DropSchema => "DROP SCHEMA {0}";

    public virtual string CreateIndex => "CREATE {0}{1}INDEX {2} ON {3} ({4})";

    public virtual string DropIndex => "DROP INDEX {0}";

    public virtual string InsertData => "INSERT INTO {0} ({1}) VALUES ({2})";

    public virtual string UpdateData => "UPDATE {0} SET {1} WHERE {2}";

    public virtual string DeleteData => "DELETE FROM {0} WHERE {1}";

    public virtual string TruncateTable => "TRUNCATE TABLE {0}";

    public virtual string CreateConstraint => "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})";

    public virtual string DeleteConstraint => "ALTER TABLE {0} DROP CONSTRAINT {1}";

    public virtual string ConvertIntegerToOrderableString => "REPLACE(STR({0}, 8), SPACE(1), '0')";

    public virtual string ConvertDateToOrderableString => "CONVERT(nvarchar, {0}, 120)";

    public virtual string ConvertDecimalToOrderableString => "REPLACE(STR({0}, 20, 9), SPACE(1), '0')";

    private DbTypes InitColumnTypeMap()
    {
        var dbTypeMap = new DbTypesFactory();
        dbTypeMap.Set<string>(DbType.String, StringColumnDefinition);
        dbTypeMap.Set<char>(DbType.StringFixedLength, StringColumnDefinition);
        dbTypeMap.Set<char?>(DbType.StringFixedLength, StringColumnDefinition);
        dbTypeMap.Set<char[]>(DbType.String, StringColumnDefinition);
        dbTypeMap.Set<bool>(DbType.Boolean, BoolColumnDefinition);
        dbTypeMap.Set<bool?>(DbType.Boolean, BoolColumnDefinition);
        dbTypeMap.Set<Guid>(DbType.Guid, GuidColumnDefinition);
        dbTypeMap.Set<Guid?>(DbType.Guid, GuidColumnDefinition);
        dbTypeMap.Set<DateTime>(DbType.DateTime, DateTimeColumnDefinition);
        dbTypeMap.Set<DateTime?>(DbType.DateTime, DateTimeColumnDefinition);
        dbTypeMap.Set<TimeSpan>(DbType.Time, TimeColumnDefinition);
        dbTypeMap.Set<TimeSpan?>(DbType.Time, TimeColumnDefinition);
        dbTypeMap.Set<DateTimeOffset>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);
        dbTypeMap.Set<DateTimeOffset?>(DbType.DateTimeOffset, DateTimeOffsetColumnDefinition);

        dbTypeMap.Set<byte>(DbType.Byte, IntColumnDefinition);
        dbTypeMap.Set<byte?>(DbType.Byte, IntColumnDefinition);
        dbTypeMap.Set<sbyte>(DbType.SByte, IntColumnDefinition);
        dbTypeMap.Set<sbyte?>(DbType.SByte, IntColumnDefinition);
        dbTypeMap.Set<short>(DbType.Int16, IntColumnDefinition);
        dbTypeMap.Set<short?>(DbType.Int16, IntColumnDefinition);
        dbTypeMap.Set<ushort>(DbType.UInt16, IntColumnDefinition);
        dbTypeMap.Set<ushort?>(DbType.UInt16, IntColumnDefinition);
        dbTypeMap.Set<int>(DbType.Int32, IntColumnDefinition);
        dbTypeMap.Set<int?>(DbType.Int32, IntColumnDefinition);
        dbTypeMap.Set<uint>(DbType.UInt32, IntColumnDefinition);
        dbTypeMap.Set<uint?>(DbType.UInt32, IntColumnDefinition);

        dbTypeMap.Set<long>(DbType.Int64, LongColumnDefinition);
        dbTypeMap.Set<long?>(DbType.Int64, LongColumnDefinition);
        dbTypeMap.Set<ulong>(DbType.UInt64, LongColumnDefinition);
        dbTypeMap.Set<ulong?>(DbType.UInt64, LongColumnDefinition);

        dbTypeMap.Set<float>(DbType.Single, RealColumnDefinition);
        dbTypeMap.Set<float?>(DbType.Single, RealColumnDefinition);
        dbTypeMap.Set<double>(DbType.Double, RealColumnDefinition);
        dbTypeMap.Set<double?>(DbType.Double, RealColumnDefinition);

        dbTypeMap.Set<decimal>(DbType.Decimal, DecimalColumnDefinition);
        dbTypeMap.Set<decimal?>(DbType.Decimal, DecimalColumnDefinition);

        dbTypeMap.Set<byte[]>(DbType.Binary, BlobColumnDefinition);

        return dbTypeMap.Create();
    }

    public virtual string GetSpecialDbType(SpecialDbType dbType, int customSize) =>
        $"{GetSpecialDbType(dbType)}({customSize})";

    protected virtual string FormatCascade(string onWhat, Rule rule)
    {
        var action = "NO ACTION";
        switch (rule)
        {
            case Rule.None:
                return string.Empty;
            case Rule.Cascade:
                action = "CASCADE";
                break;
            case Rule.SetNull:
                action = "SET NULL";
                break;
            case Rule.SetDefault:
                action = "SET DEFAULT";
                break;
        }

        return $" ON {onWhat} {action}";
    }

    protected virtual string FormatString(ColumnDefinition column) => GetQuotedColumnName(column.Name);

    protected virtual string FormatType(ColumnDefinition column)
    {
        if (column.Type.HasValue == false && string.IsNullOrEmpty(column.CustomType) == false)
        {
            return column.CustomType;
        }

        if (column.CustomDbType.HasValue)
        {
            if (column.Size != default)
            {
                return GetSpecialDbType(column.CustomDbType.Value, column.Size);
            }

            return GetSpecialDbType(column.CustomDbType.Value);
        }

        Type type = column.Type.HasValue
            ? DbTypeMap.ColumnDbTypeMap.First(x => x.Value == column.Type.Value).Key
            : column.PropertyType;

        if (type == typeof(string))
        {
            var valueOrDefault = column.Size != default ? column.Size : DefaultStringLength;
            return string.Format(StringLengthColumnDefinitionFormat, valueOrDefault);
        }

        if (type == typeof(decimal))
        {
            var precision = column.Size != default ? column.Size : DefaultDecimalPrecision;
            var scale = column.Precision != default ? column.Precision : DefaultDecimalScale;
            return string.Format(DecimalColumnDefinitionFormat, precision, scale);
        }

        var definition = DbTypeMap.ColumnTypeMap[type];
        var dbTypeDefinition = column.Size != default
            ? $"{definition}({column.Size})"
            : definition;
        //NOTE Precision is left out
        return dbTypeDefinition;
    }

    protected virtual string FormatNullable(ColumnDefinition column) => column.IsNullable ? "NULL" : "NOT NULL";

    protected virtual string FormatConstraint(ColumnDefinition column)
    {
        if (string.IsNullOrEmpty(column.ConstraintName) && column.DefaultValue == null)
        {
            return string.Empty;
        }

        return
            $"CONSTRAINT {(string.IsNullOrEmpty(column.ConstraintName) ? GetQuotedName($"DF_{column.TableName}_{column.Name}") : column.ConstraintName)}";
    }

    protected virtual string FormatDefaultValue(ColumnDefinition column)
    {
        if (column.DefaultValue == null)
        {
            return string.Empty;
        }

        // HACK: probably not needed with latest changes
        if (string.Equals(column.DefaultValue.ToString(), "GETDATE()", StringComparison.OrdinalIgnoreCase))
        {
            column.DefaultValue = SystemMethods.CurrentDateTime;
        }

        // see if this is for a system method
        if (column.DefaultValue is SystemMethods)
        {
            var method = FormatSystemMethods((SystemMethods)column.DefaultValue);
            return string.IsNullOrEmpty(method) ? string.Empty : string.Format(DefaultValueFormat, method);
        }

        return string.Format(DefaultValueFormat, GetQuotedValue(column.DefaultValue.ToString()!));
    }

    protected virtual string FormatPrimaryKey(ColumnDefinition column) => string.Empty;

    protected abstract string? FormatSystemMethods(SystemMethods systemMethod);

    protected abstract string FormatIdentity(ColumnDefinition column);
}
