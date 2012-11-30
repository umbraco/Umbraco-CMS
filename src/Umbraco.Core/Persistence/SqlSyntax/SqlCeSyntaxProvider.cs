using Umbraco.Core.Persistence.Migrations.Model;
using ColumnDefinition = Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions.ColumnDefinition;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the Sql CE SqlSyntax Providers singleton
    /// </summary>
    internal static class SqlCeSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return SqlCeSyntaxProvider.Instance; } }
    }

    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Ce
    /// </summary>
    internal class SqlCeSyntaxProvider : SqlSyntaxProviderBase<SqlCeSyntaxProvider>
    {
        public static SqlCeSyntaxProvider Instance = new SqlCeSyntaxProvider();

        private SqlCeSyntaxProvider()
        {
            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            AutoIncrementDefinition = "IDENTITY(1,1)";
            StringColumnDefinition = "NVARCHAR(255)";
            GuidColumnDefinition = "UniqueIdentifier";
            RealColumnDefinition = "FLOAT";
            BoolColumnDefinition = "BIT";
            DecimalColumnDefinition = "DECIMAL(38,6)";
            TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
            BlobColumnDefinition = "VARBINARY(MAX)";

            InitColumnTypeMap();
        }

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

        public override string GetPrimaryKeyStatement(ColumnDefinition column, string tableName)
        {
            string constraintName = string.IsNullOrEmpty(column.PrimaryKeyName)
                                        ? string.Format("PK_{0}", tableName)
                                        : column.PrimaryKeyName;

            string columns = string.IsNullOrEmpty(column.PrimaryKeyColumns)
                                 ? GetQuotedColumnName(column.ColumnName)
                                 : column.PrimaryKeyColumns;

            string sql = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2}); \n",
                                       GetQuotedTableName(tableName),
                                       GetQuotedName(constraintName),
                                       columns);

            return sql;
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName",
                                       new { TableName = tableName });

            return result > 0;
        }

        protected override string FormatIdentity(Migrations.Model.ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(Migrations.Model.ColumnDefinition column)
        {
            return "IDENTITY(1,1)";
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "NEWID()";
                case SystemMethods.NewSequentialId:
                    return "NEWSEQUENTIALID()";
                case SystemMethods.CurrentDateTime:
                    return "GETDATE()";
                case SystemMethods.CurrentUTCDateTime:
                    return "GETUTCDATE()";
            }

            return null;
        }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }
    }
}