using System.Data;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Persistence.SqlServer.Services;

/// <summary>
///     Abstract class for defining MS sql implementations
/// </summary>
/// <typeparam name="TSyntax"></typeparam>
public abstract class MicrosoftSqlSyntaxProviderBase<TSyntax> : SqlSyntaxProviderBase<TSyntax>
    where TSyntax : ISqlSyntaxProvider
{
    private readonly ILogger _logger;

    protected MicrosoftSqlSyntaxProviderBase()
    {
        _logger = StaticApplicationLogging.CreateLogger<TSyntax>();

        AutoIncrementDefinition = "IDENTITY(1,1)";
        GuidColumnDefinition = "UniqueIdentifier";
        RealColumnDefinition = "FLOAT";
        BoolColumnDefinition = "BIT";
        DecimalColumnDefinition = "DECIMAL(38,6)";
        TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
        BlobColumnDefinition = "VARBINARY(MAX)";
    }

    public override string RenameTable => "sp_rename '{0}', '{1}'";

    public override string AddColumn => "ALTER TABLE {0} ADD {1}";

    public override string GetQuotedTableName(string? tableName)
    {
        if (tableName?.Contains(".") == false)
        {
            return $"[{tableName}]";
        }

        var tableNameParts = tableName?.Split(Core.Constants.CharArrays.Period, 2);
        return $"[{tableNameParts?[0]}].[{tableNameParts?[1]}]";
    }

    public override string GetQuotedColumnName(string? columnName) => $"[{columnName}]";

    public override string GetQuotedName(string? name) => $"[{name}]";

    public override string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType)
    {
        switch (columnType)
        {
            case TextColumnType.NVarchar:
                return base.GetStringColumnEqualComparison(column, paramIndex, columnType);
            case TextColumnType.NText:
                //MSSQL doesn't allow for = comparison with NText columns but allows this syntax
                return $"{column} LIKE @{paramIndex}";
            default:
                throw new ArgumentOutOfRangeException(nameof(columnType));
        }
    }

    public override string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType)
    {
        switch (columnType)
        {
            case TextColumnType.NVarchar:
                return base.GetStringColumnWildcardComparison(column, paramIndex, columnType);
            case TextColumnType.NText:
                //MSSQL doesn't allow for upper methods with NText columns
                return $"{column} LIKE @{paramIndex}";
            default:
                throw new ArgumentOutOfRangeException(nameof(columnType));
        }
    }

    /// <summary>
    ///     This uses a the DbTypeMap created and custom mapping to resolve the SqlDbType
    /// </summary>
    /// <param name="clrType"></param>
    /// <returns></returns>
    public virtual SqlDbType GetSqlDbType(Type clrType)
    {
        DbType dbType = DbTypeMap.ColumnDbTypeMap[clrType];
        return GetSqlDbType(dbType);
    }

    /// <summary>
    ///     Returns the mapped SqlDbType for the DbType specified
    /// </summary>
    /// <param name="dbType"></param>
    /// <returns></returns>
    public virtual SqlDbType GetSqlDbType(DbType dbType)
    {
        SqlDbType sqlDbType;

        //SEE: https://msdn.microsoft.com/en-us/library/cc716729(v=vs.110).aspx
        // and https://msdn.microsoft.com/en-us/library/yy6y35y8%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396
        switch (dbType)
        {
            case DbType.AnsiString:
                sqlDbType = SqlDbType.VarChar;
                break;
            case DbType.Binary:
                sqlDbType = SqlDbType.VarBinary;
                break;
            case DbType.Byte:
                sqlDbType = SqlDbType.TinyInt;
                break;
            case DbType.Boolean:
                sqlDbType = SqlDbType.Bit;
                break;
            case DbType.Currency:
                sqlDbType = SqlDbType.Money;
                break;
            case DbType.Date:
                sqlDbType = SqlDbType.Date;
                break;
            case DbType.DateTime:
                sqlDbType = SqlDbType.DateTime;
                break;
            case DbType.Decimal:
                sqlDbType = SqlDbType.Decimal;
                break;
            case DbType.Double:
                sqlDbType = SqlDbType.Float;
                break;
            case DbType.Guid:
                sqlDbType = SqlDbType.UniqueIdentifier;
                break;
            case DbType.Int16:
                sqlDbType = SqlDbType.SmallInt;
                break;
            case DbType.Int32:
                sqlDbType = SqlDbType.Int;
                break;
            case DbType.Int64:
                sqlDbType = SqlDbType.BigInt;
                break;
            case DbType.Object:
                sqlDbType = SqlDbType.Variant;
                break;
            case DbType.SByte:
                throw new NotSupportedException("Inferring a SqlDbType from SByte is not supported.");
            case DbType.Single:
                sqlDbType = SqlDbType.Real;
                break;
            case DbType.String:
                sqlDbType = SqlDbType.NVarChar;
                break;
            case DbType.Time:
                sqlDbType = SqlDbType.Time;
                break;
            case DbType.UInt16:
                throw new NotSupportedException("Inferring a SqlDbType from UInt16 is not supported.");
            case DbType.UInt32:
                throw new NotSupportedException("Inferring a SqlDbType from UInt32 is not supported.");
            case DbType.UInt64:
                throw new NotSupportedException("Inferring a SqlDbType from UInt64 is not supported.");
            case DbType.VarNumeric:
                throw new NotSupportedException("Inferring a VarNumeric from UInt64 is not supported.");
            case DbType.AnsiStringFixedLength:
                sqlDbType = SqlDbType.Char;
                break;
            case DbType.StringFixedLength:
                sqlDbType = SqlDbType.NChar;
                break;
            case DbType.Xml:
                sqlDbType = SqlDbType.Xml;
                break;
            case DbType.DateTime2:
                sqlDbType = SqlDbType.DateTime2;
                break;
            case DbType.DateTimeOffset:
                sqlDbType = SqlDbType.DateTimeOffset;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return sqlDbType;
    }

    public override void HandleCreateTable(IDatabase database, TableDefinition tableDefinition, bool skipKeysAndIndexes = false)
    {
        var createSql = Format(tableDefinition);
        var createPrimaryKeySql = FormatPrimaryKey(tableDefinition);
        List<string> foreignSql = Format(tableDefinition.ForeignKeys);

        _logger.LogInformation("Create table:\n {Sql}", createSql);
        database.Execute(new Sql(createSql));

        if (skipKeysAndIndexes)
        {
            return;
        }

        //If any statements exists for the primary key execute them here
        if (string.IsNullOrEmpty(createPrimaryKeySql) == false)
        {
            _logger.LogInformation("Create Primary Key:\n {Sql}", createPrimaryKeySql);
            database.Execute(new Sql(createPrimaryKeySql));
        }

        List<string> indexSql = Format(tableDefinition.Indexes);
        //Loop through index statements and execute sql
        foreach (var sql in indexSql)
        {
            _logger.LogInformation("Create Index:\n {Sql}", sql);
            database.Execute(new Sql(sql));
        }

        //Loop through foreignkey statements and execute sql
        foreach (var sql in foreignSql)
        {
            _logger.LogInformation("Create Foreign Key:\n {Sql}", sql);
            database.Execute(new Sql(sql));
        }
    }
}
