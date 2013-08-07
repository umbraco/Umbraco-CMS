using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the Sql Server SqlSyntax Provider
    /// </summary>
    internal static class SqlServerSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return new SqlServerSyntaxProvider(); } }
    }

    /// <summary>
    /// Represents the version name of SQL server (i.e. the year 2008, 2005, etc...)
    /// </summary>
    internal enum SqlServerVersionName
    {
        Invalid = -1,
        V7 = 0,
        V2000 = 1,
        V2005 = 2,
        V2008 = 3,
        V2012 = 4,
        Other = 5
    }

    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Server
    /// </summary>
    [SqlSyntaxProviderAttribute("System.Data.SqlClient")]
    public class SqlServerSyntaxProvider : SqlSyntaxProviderBase<SqlServerSyntaxProvider>
    {
        public SqlServerSyntaxProvider()
        {
            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            AutoIncrementDefinition = "IDENTITY(1,1)";
            StringColumnDefinition = "VARCHAR(8000)";
            GuidColumnDefinition = "UniqueIdentifier";
            RealColumnDefinition = "FLOAT";
            BoolColumnDefinition = "BIT";
            DecimalColumnDefinition = "DECIMAL(38,6)";
            TimeColumnDefinition = "TIME"; //SQLSERVER 2008+
            BlobColumnDefinition = "VARBINARY(MAX)";

            InitColumnTypeMap();
        }

        /// <summary>
        /// Gets/sets the version of the current SQL server instance
        /// </summary>
        internal Lazy<SqlServerVersionName> VersionName { get; set; }

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

        public override IEnumerable<string> GetTablesInSchema(Database db)
        {
            var items = db.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
            return items.Select(x => x.TABLE_NAME).Cast<string>().ToList();
        }

        public override IEnumerable<ColumnInfo> GetColumnsInSchema(Database db)
        {
            var items = db.Fetch<dynamic>("SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS");
            return
                items.Select(
                    item =>
                    new ColumnInfo(item.TABLE_NAME, item.COLUMN_NAME, item.ORDINAL_POSITION, item.COLUMN_DEFAULT,
                                   item.IS_NULLABLE, item.DATA_TYPE)).ToList();
        }

        public override IEnumerable<Tuple<string, string>> GetConstraintsPerTable(Database db)
        {
            var items =
                db.Fetch<dynamic>(
                    "SELECT TABLE_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE");
            return items.Select(item => new Tuple<string, string>(item.TABLE_NAME, item.CONSTRAINT_NAME)).ToList();
        }

        public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(Database db)
        {
            var items =
                db.Fetch<dynamic>(
                    "SELECT TABLE_NAME, COLUMN_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE");
            return items.Select(item => new Tuple<string, string, string>(item.TABLE_NAME, item.COLUMN_NAME, item.CONSTRAINT_NAME)).ToList();
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName",
                                       new { TableName = tableName });

            return result > 0;
        }

        public override string FormatColumnRename(string tableName, string oldName, string newName)
        {
            return string.Format(RenameColumn, tableName, oldName, newName);
        }

        public override string FormatTableRename(string oldName, string newName)
        {
            return string.Format(RenameTable, oldName, newName);
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(ColumnDefinition column)
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

        public override string DeleteDefaultConstraint
        {
            get { return "ALTER TABLE [{0}] DROP CONSTRAINT [DF_{0}_{1}]"; }
        }

        public override string AddColumn { get { return "ALTER TABLE {0} ADD {1}"; } }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string RenameColumn { get { return "sp_rename '{0}.{1}', '{2}', 'COLUMN'"; } }

        public override string RenameTable { get { return "sp_rename '{0}', '{1}'"; } }
    }
}