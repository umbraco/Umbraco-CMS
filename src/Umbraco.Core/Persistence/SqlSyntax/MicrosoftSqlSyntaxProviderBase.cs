using System;
using System.Data;
using System.Linq;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Abstract class for defining MS sql implementations
    /// </summary>
    /// <typeparam name="TSyntax"></typeparam>
    public abstract class MicrosoftSqlSyntaxProviderBase<TSyntax> : SqlSyntaxProviderBase<TSyntax>
        where TSyntax : ISqlSyntaxProvider
    {
        protected MicrosoftSqlSyntaxProviderBase()
        {            
            AutoIncrementDefinition = "IDENTITY(1,1)";
            GuidColumnDefinition = "UniqueIdentifier";
            RealColumnDefinition = "FLOAT";
            BoolColumnDefinition = "BIT";
            DecimalColumnDefinition = "DECIMAL(38,6)";
            TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
            BlobColumnDefinition = "VARBINARY(MAX)";

            InitColumnTypeMap();
        }

        public override string RenameTable { get { return "sp_rename '{0}', '{1}'"; } }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }

        public override string GetQuotedTableName(string tableName)
        {
            if (tableName.Contains(".") == false)
                return string.Format("[{0}]", tableName);

            var tableNameParts = tableName.Split(new[] { '.' }, 2);
            return string.Format("[{0}].[{1}]", tableNameParts[0], tableNameParts[1]);
        }

        public override string GetQuotedColumnName(string columnName)
        {
            return string.Format("[{0}]", columnName);
        }

        public override string GetQuotedName(string name)
        {
            return string.Format("[{0}]", name);
        }

        public override string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType)
        {
            switch (columnType)
            {
                case TextColumnType.NVarchar:
                    return base.GetStringColumnEqualComparison(column, paramIndex, columnType);
                case TextColumnType.NText:
                    //MSSQL doesn't allow for = comparison with NText columns but allows this syntax
                    return string.Format("{0} LIKE @{1}", column, paramIndex);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
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
                    return string.Format("{0} LIKE @{1}", column, paramIndex);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
            }
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public override string GetStringColumnStartsWithComparison(string column, string value, TextColumnType columnType)
        {
            switch (columnType)
            {
                case TextColumnType.NVarchar:
                    return base.GetStringColumnStartsWithComparison(column, value, columnType);
                case TextColumnType.NText:
                    //MSSQL doesn't allow for upper methods with NText columns
                    return string.Format("{0} LIKE '{1}%'", column, value);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
            }
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public override string GetStringColumnEndsWithComparison(string column, string value, TextColumnType columnType)
        {
            switch (columnType)
            {
                case TextColumnType.NVarchar:
                    return base.GetStringColumnEndsWithComparison(column, value, columnType);
                case TextColumnType.NText:
                    //MSSQL doesn't allow for upper methods with NText columns
                    return string.Format("{0} LIKE '%{1}'", column, value);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
            }
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public override string GetStringColumnContainsComparison(string column, string value, TextColumnType columnType)
        {
            switch (columnType)
            {
                case TextColumnType.NVarchar:
                    return base.GetStringColumnContainsComparison(column, value, columnType);
                case TextColumnType.NText:
                    //MSSQL doesn't allow for upper methods with NText columns
                    return string.Format("{0} LIKE '%{1}%'", column, value);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
            }
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public override string GetStringColumnWildcardComparison(string column, string value, TextColumnType columnType)
        {
            switch (columnType)
            {
                case TextColumnType.NVarchar:
                    return base.GetStringColumnContainsComparison(column, value, columnType);
                case TextColumnType.NText:
                    //MSSQL doesn't allow for upper methods with NText columns
                    return string.Format("{0} LIKE '{1}'", column, value);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
            }
        }

        /// <summary>
        /// This uses a the DbTypeMap created and custom mapping to resolve the SqlDbType
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public virtual SqlDbType GetSqlDbType(Type clrType)
        {
            var dbType = DbTypeMap.ColumnDbTypeMap.First(x => x.Key == clrType).Value;
            return GetSqlDbType(dbType);
        }

        /// <summary>
        /// Returns the mapped SqlDbType for the DbType specified
        /// </summary>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public virtual SqlDbType GetSqlDbType(DbType dbType)
        {
            var sqlDbType = SqlDbType.NVarChar;

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
    }
}