using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Persistence.SqlServer.Dtos;
using Umbraco.Extensions;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     Represents an SqlSyntaxProvider for Sql Server.
/// </summary>
public class SqlServerSyntaxProvider : MicrosoftSqlSyntaxProviderBase<SqlServerSyntaxProvider>
{
    public enum EngineEdition
    {
        Unknown = 0,
        Desktop = 1,
        Standard = 2,
        Enterprise = 3, // Also developer edition
        Express = 4,
        Azure = 5,
    }

    public enum VersionName
    {
        Invalid = -1,
        Unknown = 0,
        V7 = 1,
        V2000 = 2,
        V2005 = 3,
        V2008 = 4,
        V2012 = 5,
        V2014 = 6,
        V2016 = 7,
        V2017 = 8,
        V2019 = 9,
        Other = 99,
    }

    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<SqlServerSyntaxProvider> _logger;

    public SqlServerSyntaxProvider(IOptions<GlobalSettings> globalSettings)
        : this(globalSettings, StaticApplicationLogging.CreateLogger<SqlServerSyntaxProvider>())
    {
    }

    public SqlServerSyntaxProvider(IOptions<GlobalSettings> globalSettings, ILogger<SqlServerSyntaxProvider> logger)
    {
        _globalSettings = globalSettings;
        _logger = logger;
    }

    public override string ProviderName => Constants.ProviderName;

    public ServerVersionInfo? ServerVersion { get; private set; }

    public override string DbProvider => ServerVersion?.IsAzure ?? false ? "SqlAzure" : "SqlServer";

    public override IsolationLevel DefaultIsolationLevel => IsolationLevel.ReadCommitted;

    public override string DeleteDefaultConstraint => "ALTER TABLE {0} DROP CONSTRAINT {2}";

    public override string DropIndex => "DROP INDEX {0} ON {1}";

    public override string RenameColumn => "sp_rename '{0}.{1}', '{2}', 'COLUMN'";

    public override string CreateIndex => "CREATE {0}{1}INDEX {2} ON {3} ({4}){5}";

    public override DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString)
    {
        var setting = _globalSettings.Value.DatabaseFactoryServerVersion;
        var fromSettings = false;

        if (setting.IsNullOrWhiteSpace() || !setting.StartsWith("SqlServer.")
                                         || !Enum<VersionName>.TryParse(setting.Substring("SqlServer.".Length), out VersionName versionName, true))
        {
            versionName = GetSetVersion(connectionString, ProviderName, _logger).ProductVersionName;
        }

        _logger.LogDebug("SqlServer {SqlServerVersion}, DatabaseType is {DatabaseType} ({Source}).", versionName, DatabaseType.SqlServer2012, fromSettings ? "settings" : "detected");

        return DatabaseType.SqlServer2012;
    }

    private static VersionName MapProductVersion(string productVersion)
    {
        var firstPart = string.IsNullOrWhiteSpace(productVersion)
            ? "??"
            : productVersion.Split(Core.Constants.CharArrays.Period)[0];
        switch (firstPart)
        {
            case "??":
                return VersionName.Invalid;
            case "15":
                return VersionName.V2019;
            case "14":
                return VersionName.V2017;
            case "13":
                return VersionName.V2016;
            case "12":
                return VersionName.V2014;
            case "11":
                return VersionName.V2012;
            case "10":
                return VersionName.V2008;
            case "9":
                return VersionName.V2005;
            case "8":
                return VersionName.V2000;
            case "7":
                return VersionName.V7;
            default:
                return VersionName.Other;
        }
    }

    internal ServerVersionInfo GetSetVersion(string? connectionString, string? providerName, ILogger logger)
    {
        // var factory = DbProviderFactories.GetFactory(providerName);
        SqlClientFactory? factory = SqlClientFactory.Instance;
        DbConnection? connection = factory.CreateConnection();

        if (connection == null)
        {
            throw new InvalidOperationException($"Could not create a connection for provider \"{providerName}\".");
        }

        // Edition: "Express Edition", "Windows Azure SQL Database..."
        // EngineEdition: 1/Desktop 2/Standard 3/Enterprise 4/Express 5/Azure
        // ProductLevel: RTM, SPx, CTP...

        const string sql = @"select
    SERVERPROPERTY('Edition') Edition,
    SERVERPROPERTY('EditionID') EditionId,
    SERVERPROPERTY('InstanceName') InstanceName,
    SERVERPROPERTY('ProductVersion') ProductVersion,
    SERVERPROPERTY('BuildClrVersion') BuildClrVersion,
    SERVERPROPERTY('EngineEdition') EngineEdition,
    SERVERPROPERTY('IsClustered') IsClustered,
    SERVERPROPERTY('MachineName') MachineName,
    SERVERPROPERTY('ResourceLastUpdateDateTime') ResourceLastUpdateDateTime,
    SERVERPROPERTY('ProductLevel') ProductLevel;";

        string GetString(IDataReader reader, int ordinal, string defaultValue)
        {
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);
        }

        int GetInt32(IDataReader reader, int ordinal, int defaultValue)
        {
            return reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);
        }

        connection.ConnectionString = connectionString;
        ServerVersionInfo version;
        using (connection)
        {
            try
            {
                connection.Open();
                DbCommand command = connection.CreateCommand();
                command.CommandText = sql;
                using (DbDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    // InstanceName can be NULL for the default instance
                    version = new ServerVersionInfo(
                        GetString(reader, 0, "Unknown"),
                        GetString(reader, 2, "(default)"),
                        GetString(reader, 3, string.Empty),
                        (EngineEdition)GetInt32(reader, 5, 0),
                        GetString(reader, 7, "DEFAULT"),
                        GetString(reader, 9, "Unknown"));
                }

                connection.Close();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to detected SqlServer version.");
                version = new ServerVersionInfo(); // all unknown
            }
        }

        return ServerVersion = version;
    }

    /// <summary>
    ///     SQL Server stores default values assigned to columns as constraints, it also stores them with named values, this is
    ///     the only
    ///     server type that does this, therefore this method doesn't exist on any other syntax provider
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Tuple<string, string, string, string>> GetDefaultConstraintsPerColumn(IDatabase db)
    {
        List<DefaultConstraintPerColumnDto>? items = db.Fetch<DefaultConstraintPerColumnDto>(
            "SELECT TableName = t.Name, ColumnName = c.Name, dc.Name, dc.[Definition] FROM sys.tables t INNER JOIN sys.default_constraints dc ON t.object_id = dc.parent_object_id INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND c.column_id = dc.parent_column_id INNER JOIN sys.schemas as s on t.[schema_id] = s.[schema_id] WHERE s.name = (SELECT SCHEMA_NAME())");
        return items.Select(x =>
            new Tuple<string, string, string, string>(x.TableName, x.ColumnName, x.Name, x.Definition));
    }

    public override IEnumerable<string> GetTablesInSchema(IDatabase db) => db.Fetch<string>(
        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");

    public override IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db)
    {
        List<ColumnInSchemaDto>? items = db.Fetch<ColumnInSchemaDto>(
            "SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
        return
            items.Select(
                item =>
                    new ColumnInfo(item.TableName, item.ColumnName, item.OrdinalPosition, item.ColumnDefault, item.IsNullable, item.DataType)).ToList();
    }

    /// <inheritdoc />
    public override IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db)
    {
        List<ConstraintPerTableDto> items =
            db.Fetch<ConstraintPerTableDto>(
                "SELECT TABLE_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
        return items.Select(item => new Tuple<string, string>(item.TableName, item.ConstraintName)).ToList();
    }

    /// <inheritdoc />
    public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db)
    {
        List<ConstraintPerColumnDto>? items =
            db.Fetch<ConstraintPerColumnDto>(
                "SELECT TABLE_NAME, COLUMN_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
        return items.Select(item =>
            new Tuple<string, string, string>(item.TableName, item.ColumnName, item.ConstraintName)).ToList();
    }

    /// <inheritdoc />
    public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
    {
        List<DefinedIndexDto>? items =
            db.Fetch<DefinedIndexDto>(
                @"select T.name as TABLE_NAME, I.name as INDEX_NAME, AC.Name as COLUMN_NAME,
CASE WHEN I.is_unique_constraint = 1 OR  I.is_unique = 1 THEN 1 ELSE 0 END AS [UNIQUE]
from sys.tables as T inner join sys.indexes as I on T.[object_id] = I.[object_id]
   inner join sys.index_columns as IC on IC.[object_id] = I.[object_id] and IC.[index_id] = I.[index_id]
   inner join sys.all_columns as AC on IC.[object_id] = AC.[object_id] and IC.[column_id] = AC.[column_id]
   inner join sys.schemas as S on T.[schema_id] = S.[schema_id]
WHERE S.name = (SELECT SCHEMA_NAME()) AND I.is_primary_key = 0
order by T.name, I.name");
        return items.Select(item => new Tuple<string, string, string, bool>(item.TableName, item.IndexName, item.ColumnName, item.Unique == 1)).ToList();
    }

    /// <inheritdoc />
    public override bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName)
    {
        constraintName = db.Fetch<string>(
                @"select con.[name] as [constraintName]
from sys.default_constraints con
join sys.columns col on con.object_id=col.default_object_id
join sys.tables tbl on col.object_id=tbl.object_id
where tbl.[name]=@0 and col.[name]=@1;",
                tableName,
                columnName)
            .FirstOrDefault();
        return !constraintName.IsNullOrWhiteSpace();
    }

    public override bool DoesTableExist(IDatabase db, string tableName)
    {
        var result =
            db.ExecuteScalar<long>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = (SELECT SCHEMA_NAME())",
                new { TableName = tableName });

        return result > 0;
    }

    public override string FormatColumnRename(string? tableName, string? oldName, string? newName) =>
        string.Format(RenameColumn, tableName, oldName, newName);

    public override string FormatTableRename(string? oldName, string? newName) =>
        string.Format(RenameTable, oldName, newName);

    protected override string FormatIdentity(ColumnDefinition column) =>
        column.IsIdentity ? GetIdentityString(column) : string.Empty;

    public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top) => new Sql<ISqlContext>(sql.SqlContext, sql.SQL.Insert(sql.SQL.IndexOf(' '), " TOP " + top), sql.Arguments);

    private static string GetIdentityString(ColumnDefinition column) => "IDENTITY(1,1)";

    protected override string? FormatSystemMethods(SystemMethods systemMethod)
    {
        switch (systemMethod)
        {
            case SystemMethods.NewGuid:
                return "NEWID()";
            case SystemMethods.CurrentDateTime:
                return "GETDATE()";

                // case SystemMethods.NewSequentialId:
                //    return "NEWSEQUENTIALID()";
                // case SystemMethods.CurrentUTCDateTime:
                //    return "GETUTCDATE()";
        }

        return null;
    }

    public override string Format(IndexDefinition index)
    {
        var name = string.IsNullOrEmpty(index.Name)
            ? $"IX_{index.TableName}_{index.ColumnName}"
            : index.Name;

        var columns = index.Columns.Any()
            ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
            : GetQuotedColumnName(index.ColumnName);

        var includeColumns = index.IncludeColumns?.Any() ?? false
            ? $" INCLUDE ({string.Join(",", index.IncludeColumns.Select(x => GetQuotedColumnName(x.Name)))})"
            : string.Empty;

        return string.Format(CreateIndex, GetIndexType(index.IndexType), " ", GetQuotedName(name), GetQuotedTableName(index.TableName), columns, includeColumns);
    }


    public override Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql)
    {
        // go find the first FROM clause, and append the lock hint
        Sql s = sql;
        var updated = false;

        while (s != null)
        {
            var sqlText = SqlInspector.GetSqlText(s);
            if (sqlText.StartsWith("FROM ", StringComparison.OrdinalIgnoreCase))
            {
                SqlInspector.SetSqlText(s, sqlText + " WITH (UPDLOCK)");
                updated = true;
                break;
            }

            s = SqlInspector.GetSqlRhs(sql);
        }

        if (updated)
        {
            SqlInspector.Reset(sql);
        }

        return sql;
    }

    public override Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql)
        => sql.Append(" WITH (UPDLOCK) ");

    public override Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
        Sql<ISqlContext> sql,
        Func<Sql<ISqlContext>,
            Sql<ISqlContext>> nestedJoin,
        string? alias = null)
    {
        Type type = typeof(TDto);

        var tableName = GetQuotedTableName(type.GetTableName());
        var join = tableName;

        if (alias != null)
        {
            var quotedAlias = GetQuotedTableName(alias);
            join += " " + quotedAlias;
        }

        var nestedSql = new Sql<ISqlContext>(sql.SqlContext);
        nestedSql = nestedJoin(nestedSql);

        Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin = sql.LeftJoin(join);
        sql.Append(nestedSql);
        return sqlJoin;
    }

    public class ServerVersionInfo
    {
        public ServerVersionInfo()
        {
            ProductVersionName = VersionName.Unknown;
            EngineEdition = EngineEdition.Unknown;
        }

        public ServerVersionInfo(string edition, string instanceName, string productVersion, EngineEdition engineEdition, string machineName, string productLevel)
        {
            Edition = edition;
            InstanceName = instanceName;
            ProductVersion = productVersion;
            ProductVersionName = MapProductVersion(ProductVersion);
            EngineEdition = engineEdition;
            MachineName = machineName;
            ProductLevel = productLevel;
        }

        public string? Edition { get; }

        public string? InstanceName { get; }

        public string? ProductVersion { get; }

        public VersionName ProductVersionName { get; }

        public EngineEdition EngineEdition { get; }

        public bool IsAzure => EngineEdition == EngineEdition.Azure;

        public string? MachineName { get; }

        public string? ProductLevel { get; }
    }

    #region Sql Inspection

    private static SqlInspectionUtilities? _sqlInspector;

    private static SqlInspectionUtilities SqlInspector =>
_sqlInspector ??= new SqlInspectionUtilities();

    private class SqlInspectionUtilities
    {
        private readonly Func<Sql, Sql> _getSqlRhs;
        private readonly Func<Sql, string> _getSqlText;
        private readonly Action<Sql, string?> _setSqlFinal;
        private readonly Action<Sql, string> _setSqlText;

        public SqlInspectionUtilities()
        {
            (_getSqlText, _setSqlText) = ReflectionUtilities.EmitFieldGetterAndSetter<Sql, string>("_sql");
            _getSqlRhs = ReflectionUtilities.EmitFieldGetter<Sql, Sql>("_rhs");
            _setSqlFinal = ReflectionUtilities.EmitFieldSetter<Sql, string?>("_sqlFinal");
        }

        public string GetSqlText(Sql sql) => _getSqlText(sql);

        public void SetSqlText(Sql sql, string sqlText) => _setSqlText(sql, sqlText);

        public Sql GetSqlRhs(Sql sql) => _getSqlRhs(sql);

        public void Reset(Sql sql) => _setSqlFinal(sql, null);
    }

    #endregion
}
