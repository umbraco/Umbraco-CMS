using System;
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
            return string.Format("[{0}]", tableName);
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
    }
}