using Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    internal static class MySqlSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return MySqlSyntaxProvider.Instance; } }
    }

    internal class MySqlSyntaxProvider : SqlSyntaxProviderBase<MySqlSyntaxProvider>
    {
        public static MySqlSyntaxProvider Instance = new MySqlSyntaxProvider();

        private MySqlSyntaxProvider()
        {
            AutoIncrementDefinition = "AUTO_INCREMENT";
            IntColumnDefinition = "int(11)";
            BoolColumnDefinition = "tinyint(1)";
            TimeColumnDefinition = "time";
            DecimalColumnDefinition = "decimal(38,6)";
            GuidColumnDefinition = "char(32)";
            DefaultStringLength = 255;
            
            InitColumnTypeMap();

            DefaultValueFormat = " DEFAULT '{0}'";
        }

        public override string GetQuotedTableName(string tableName)
        {
            return string.Format("`{0}`", tableName);
        }

        public override string GetQuotedColumnName(string columnName)
        {
            return string.Format("`{0}`", columnName);
        }

        public override string GetQuotedName(string name)
        {
            return string.Format("`{0}`", name);
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_NAME = @TableName AND " +
                "TABLE_SCHEMA = @TableSchema", new { TableName = tableName, TableSchema = db.Connection.Database });

            return result > 0;
        }
    }
}