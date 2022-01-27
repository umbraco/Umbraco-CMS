using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Persistence.Sqlite.Services;

/// <summary>
/// Implements <see cref="ISqlSyntaxProvider"/> for SQLite.
/// </summary>
public class SqliteSyntaxProvider : SqlSyntaxProviderBase<SqliteSyntaxProvider>
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<SqliteSyntaxProvider> _log;

    public SqliteSyntaxProvider(IOptions<GlobalSettings> globalSettings, ILogger<SqliteSyntaxProvider> log)
    {
        _globalSettings = globalSettings;
        _log = log;
    }

    /// <inheritdoc />
    public override string ProviderName => Constants.ProviderName;

    public override string StringColumnDefinition => "TEXT COLLATE NOCASE";

    public override string StringLengthUnicodeColumnDefinitionFormat => "TEXT COLLATE NOCASE";

    /// <inheritdoc />
    public override IsolationLevel DefaultIsolationLevel => IsolationLevel.Serializable;

    /// <inheritdoc />
    public override string DbProvider => Constants.ProviderName;


    /// <inheritdoc />
    public override bool SupportsIdentityInsert() => false;

    /// <inheritdoc />
    public override bool SupportsClustered() => false;



    public override string GetIndexType(IndexTypes indexTypes)
    {
        switch (indexTypes)
        {
            case IndexTypes.UniqueNonClustered:
                return "UNIQUE";
            default:
                return string.Empty;
        }
    }

    public override List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys)
    {
        return foreignKeys.Select(Format).ToList();
    }

    public virtual string Format(ForeignKeyDefinition foreignKey)
    {
        var constraintName = string.IsNullOrEmpty(foreignKey.Name)
            ? $"FK_{foreignKey.ForeignTable}_{foreignKey.PrimaryTable}_{foreignKey.PrimaryColumns.First()}"
            : foreignKey.Name;

        var localColumn = GetQuotedColumnName(foreignKey.ForeignColumns.First());
        var remoteColumn = GetQuotedColumnName(foreignKey.PrimaryColumns.First());
        var remoteTable = GetQuotedTableName(foreignKey.PrimaryTable);
        var onDelete = FormatCascade("DELETE", foreignKey.OnDelete);
        var onUpdate = FormatCascade("UPDATE", foreignKey.OnUpdate);

        return
            $"CONSTRAINT {constraintName} FOREIGN KEY ({localColumn}) REFERENCES {remoteTable} ({remoteColumn}) {onDelete} {onUpdate}";
    }

    /// <inheritdoc />
    public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
    {
        List<IndexMeta> items = db.Fetch<IndexMeta>(
            @"SELECT
                m.tbl_name AS tableName,
                ilist.name AS indexName,
                iinfo.name AS columnName,
                ilist.[unique] AS isUnique
            FROM
                sqlite_master AS m,
                pragma_index_list(m.name) AS ilist,
                pragma_index_info(ilist.name) AS iinfo");

        return items.Select(item =>
                new Tuple<string, string, string, bool>(item.TableName, item.IndexName, item.ColumnName, item.IsUnique))
            .ToList();
    }


    public override string ConvertIntegerToOrderableString => "substr('0000000000'||'{0}', -10, 10)";
    public override string ConvertDecimalToOrderableString => "substr('0000000000'||'{0}', -10, 10)";
    public override string ConvertDateToOrderableString => "{0}";

    /// <inheritdoc />
    public override string GetSpecialDbType(SpecialDbType dbType) => "TEXT COLLATE NOCASE";

    /// <inheritdoc />
    public override string GetSpecialDbType(SpecialDbType dbType, int customSize) => GetSpecialDbType(dbType);

    /// <inheritdoc />
    public override bool TryGetDefaultConstraint(IDatabase db, string tableName, string columnName,
        out string constraintName)
    {
        // TODO: SQLite
        constraintName = string.Empty;
        return false;
    }

    /// <inheritdoc />
    public override void WriteLock(IDatabase db, TimeSpan timeout, int lockId) => ObtainWriteLock(db, timeout, lockId);

    /// <inheritdoc />
    public override void WriteLock(IDatabase db, params int[] lockIds)
    {
        TimeSpan timeout = _globalSettings.Value.SqlWriteLockTimeOut;

        foreach (var lockId in lockIds)
        {
            ObtainWriteLock(db, timeout, lockId);
        }
    }

    /// <inheritdoc />
    public override void ReadLock(IDatabase db, TimeSpan timeout, int lockId) => ObtainReadLock(db, timeout, lockId);

    /// <inheritdoc />
    public override void ReadLock(IDatabase db, params int[] lockIds)
    {
        foreach (var lockId in lockIds)
        {
            ObtainReadLock(db, null, lockId);
        }
    }

    private static void ObtainReadLock(IDatabase db, TimeSpan? timeout, int lockId)
    {
        if (db is null)
        {
            throw new ArgumentNullException(nameof(db));
        }

        if (db.Transaction is null)
        {
            throw new ArgumentException(nameof(db) + "." + nameof(db.Transaction) + " is null");
        }

        if (db.Transaction.IsolationLevel < IsolationLevel.Serializable)
        {
            throw new InvalidOperationException("A transaction with minimum Serializable isolation level is required.");
        }

        if (timeout.HasValue)
        {
            db.Execute(@$"PRAGMA busy_timeout = {timeout.Value.TotalMilliseconds};");
        }

        var i = db.ExecuteScalar<int?>("SELECT value FROM umbracoLock WHERE id=@id", new {id = lockId});

        // ensure we are actually locking!
        if (i == null)
        {
            throw new ArgumentException($"LockObject with id={lockId} does not exist.");
        }
    }

    private static void ObtainWriteLock(IDatabase db, TimeSpan timeout, int lockId)
    {
        if (db is null)
        {
            throw new ArgumentNullException(nameof(db));
        }

        if (db.Transaction is null)
        {
            throw new ArgumentException(nameof(db) + "." + nameof(db.Transaction) + " is null");
        }

        if (db.Transaction.IsolationLevel < IsolationLevel.Serializable)
        {
            throw new InvalidOperationException("A transaction with minimum Serializable isolation level is required.");
        }

        db.Execute(@$"PRAGMA busy_timeout = {timeout.TotalMilliseconds};");

        var i = db.Execute(@"UPDATE umbracoLock SET value = (CASE WHEN (value=1) THEN -1 ELSE 1 END) WHERE id=@id", new { id = lockId });

        // ensure we are actually locking!
        if (i == 0)
        {
            throw new ArgumentException($"LockObject with id={lockId} does not exist.");
        }
    }

    public override string GetFieldNameForUpdate<TDto>(Expression<Func<TDto, object>> fieldSelector,
        string tableAlias = null)
    {
        var field = ExpressionHelper.FindProperty(fieldSelector).Item1 as PropertyInfo;
        var fieldName = GetColumnName(field!);

        return GetQuotedColumnName(fieldName);
    }

    private static string GetColumnName(PropertyInfo column)
    {
        ColumnAttribute? attr = column.FirstAttribute<ColumnAttribute>();
        return string.IsNullOrWhiteSpace(attr?.Name) ? column.Name : attr.Name;
    }

    /// <inheritdoc />
    protected override string FormatSystemMethods(SystemMethods systemMethod)
    {
        // TODO: SQLite
        switch (systemMethod)
        {
            case SystemMethods.NewGuid:
                return "NEWID()"; // No NEWID() in SQLite perhaps try RANDOM()
            case SystemMethods.CurrentDateTime:
                return "DATE()"; // No GETDATE() trying DATE()
        }

        return null;
    }

    /// <inheritdoc />
    protected override string FormatIdentity(ColumnDefinition column)
    {
        /* NOTE: We need AUTOINCREMENT, adds overhead but makes magic ids not break everything.
         * e.g. Cms.Core.Constants.Security.SuperUserId is -1
         * without the sqlite_sequence table we end up with the next user id = 0
         * but 0 is considered to not exist by our c# code and things explode */
        return column.IsIdentity ? "PRIMARY KEY AUTOINCREMENT" : string.Empty;
    }

    public override string GetConcat(params string[] args)
    {
        return string.Join(" || ", args.AsEnumerable());
    }

    public override string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string referenceName = null, bool forInsert = false)
    {
        if (forInsert)
        {
            return dbType.EscapeSqlIdentifier(columnName);
        }

        return base.GetColumn(dbType, tableName, columnName, columnAlias, referenceName, forInsert);
    }

    public override string FormatPrimaryKey(TableDefinition table)
    {
        ColumnDefinition? columnDefinition = table.Columns.FirstOrDefault(x => x.IsPrimaryKey);
        if (columnDefinition == null)
        {
            return string.Empty;
        }

        if (table.Columns.Any(x => x.IsIdentity))
        {
            return string.Empty;
        }

        var constraintName = string.IsNullOrEmpty(columnDefinition.PrimaryKeyName)
            ? $"PK_{table.Name}"
            : columnDefinition.PrimaryKeyName;

        var columns = string.IsNullOrEmpty(columnDefinition.PrimaryKeyColumns)
            ? GetQuotedColumnName(columnDefinition.Name)
            : string.Join(", ", columnDefinition.PrimaryKeyColumns
                .Split(Cms.Core.Constants.CharArrays.CommaSpace, StringSplitOptions.RemoveEmptyEntries)
                .Select(GetQuotedColumnName));

        return $"CONSTRAINT {constraintName} PRIMARY KEY ({columns})";
    }


    /// <inheritdoc />
    public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top)
    {
        // SQLite uses LIMIT as opposed to TOP
        // SELECT TOP 5 * FROM My_Table
        // SELECT * FROM My_Table LIMIT 5;

        return sql.Append($"LIMIT {top}");
    }

    public virtual string Format(IEnumerable<ColumnDefinition> columns)
    {
        var sb = new StringBuilder();
        foreach (ColumnDefinition column in columns)
        {
            sb.AppendLine(", " + Format(column));
        }

        return sb.ToString().TrimStart(',');
    }

    public override void HandleCreateTable(IDatabase database, TableDefinition tableDefinition)
    {
        var columns = Format(tableDefinition.Columns);
        var primaryKey = FormatPrimaryKey(tableDefinition);
        List<string> foreignKeys = Format(tableDefinition.ForeignKeys);

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE TABLE {tableDefinition.Name}");
        sb.AppendLine("(");
        sb.Append(columns);

        if (!string.IsNullOrEmpty(primaryKey))
        {
            sb.AppendLine($", {primaryKey}");
        }

        foreach (var foreignKey in foreignKeys)
        {
            sb.AppendLine($", {foreignKey}");
        }

        sb.AppendLine(")");

        var createSql = sb.ToString();

        _log.LogInformation("Create table:\n {Sql}", createSql);
        database.Execute(new Sql(createSql));

        List<string> indexSql = Format(tableDefinition.Indexes);
        foreach (var sql in indexSql)
        {
            _log.LogInformation("Create Index:\n {Sql}", sql);
            database.Execute(new Sql(sql));
        }
    }

    public override IEnumerable<string> GetTablesInSchema(IDatabase db)
    {
        return db.Fetch<string>("select name from sqlite_master where type='table'");
    }

    public override IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db)
    {
        IEnumerable<string> tables = GetTablesInSchema(db);

        db.OpenSharedConnection();
        foreach (var table in tables)
        {
            DbCommand? cmd = db.CreateCommand(db.Connection, CommandType.Text, $"PRAGMA table_info({table})");
            DbDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var cid = reader.GetInt32("cid");
                var column = reader.GetString("name");
                var type = reader.GetString("type");
                var notNull = reader.GetBoolean("notnull");
                yield return new ColumnInfo(table, column, cid, notNull, type);
            }
        }
    }

    private class IndexMeta
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public string ColumnName { get; set; }
        public bool IsUnique { get; set; }
    }
}
