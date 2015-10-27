using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Server
    /// </summary>
    [SqlSyntaxProviderAttribute("System.Data.SqlClient")]
    public class SqlServerSyntaxProvider : MicrosoftSqlSyntaxProviderBase<SqlServerSyntaxProvider>
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

        /// <summary>
        /// SQL Server stores default values assigned to columns as constraints, it also stores them with named values, this is the only
        /// server type that does this, therefore this method doesn't exist on any other syntax provider
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string, string, string>> GetDefaultConstraintsPerColumn(Database db)
        {
            var items = db.Fetch<dynamic>("SELECT TableName = t.Name,ColumnName = c.Name,dc.Name,dc.[Definition] FROM sys.tables t INNER JOIN sys.default_constraints dc ON t.object_id = dc.parent_object_id INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND c.column_id = dc.parent_column_id");
            return items.Select(x => new Tuple<string, string, string, string>(x.TableName, x.ColumnName, x.Name, x.Definition));
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

        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(Database db)
        {
            var items =
                db.Fetch<dynamic>(
                    @"select T.name as TABLE_NAME, I.name as INDEX_NAME, AC.Name as COLUMN_NAME,
CASE WHEN I.is_unique_constraint = 1 OR  I.is_unique = 1 THEN 1 ELSE 0 END AS [UNIQUE]
from sys.tables as T inner join sys.indexes as I on T.[object_id] = I.[object_id] 
   inner join sys.index_columns as IC on IC.[object_id] = I.[object_id] and IC.[index_id] = I.[index_id] 
   inner join sys.all_columns as AC on IC.[object_id] = AC.[object_id] and IC.[column_id] = AC.[column_id] 
WHERE I.name NOT LIKE 'PK_%'
order by T.name, I.name");
            return items.Select(item => new Tuple<string, string, string, bool>(item.TABLE_NAME, item.INDEX_NAME, item.COLUMN_NAME, 
                item.UNIQUE == 1)).ToList();
            
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
                case SystemMethods.CurrentDateTime:
                    return "GETDATE()";
                //case SystemMethods.NewSequentialId:
                //    return "NEWSEQUENTIALID()";
                //case SystemMethods.CurrentUTCDateTime:
                //    return "GETUTCDATE()";
            }

            return null;
        }

        public override string DeleteDefaultConstraint
        {
            get { return "ALTER TABLE [{0}] DROP CONSTRAINT [DF_{0}_{1}]"; }
        }

        
        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string RenameColumn { get { return "sp_rename '{0}.{1}', '{2}', 'COLUMN'"; } }

        
    }
}