using System;
using System.Data;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Querying;

namespace Umbraco.Cms.Infrastructure.Persistence.SqlSyntax
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
        }

        public override string RenameTable => "sp_rename '{0}', '{1}'";

        public override string AddColumn => "ALTER TABLE {0} ADD {1}";

        public override string GetQuotedTableName(string tableName)
        {
            if (tableName.Contains(".") == false)
                return $"[{tableName}]";

            var tableNameParts = tableName.Split(Constants.CharArrays.Period, 2);
            return $"[{tableNameParts[0]}].[{tableNameParts[1]}]";
        }

        public override string GetQuotedColumnName(string columnName) => $"[{columnName}]";

        public override string GetQuotedName(string name) => $"[{name}]";

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

    }
}
