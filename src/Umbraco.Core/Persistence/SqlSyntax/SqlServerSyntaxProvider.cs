using System;
using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Server
    /// </summary>
    [SqlSyntaxProvider(Constants.DbProviderNames.SqlServer)]
    public class SqlServerSyntaxProvider : MicrosoftSqlSyntaxProviderBase<SqlServerSyntaxProvider>
    {
        // IDatabaseFactory to be lazily injected
        public SqlServerSyntaxProvider(Lazy<IDatabaseFactory> lazyFactory)
        {
            _serverVersion = new Lazy<ServerVersionInfo>(() =>
            {
                var factory = lazyFactory.Value;
                if (factory == null)
                    throw new InvalidOperationException("Failed to determine Sql Server version (no database factory).");
                return DetermineVersion(factory);
            });
        }

        private readonly Lazy<ServerVersionInfo> _serverVersion;

        internal ServerVersionInfo ServerVersion => _serverVersion.Value;

        internal enum VersionName
        {
            Invalid = -1,
            Unknown = 0,
            V7 = 1,
            V2000 = 2,
            V2005 = 3,
            V2008 = 4,
            V2012 = 5,
            V2014 = 6,
            Other = 99
        }

        internal enum EngineEdition
        {
            Unknown = 0,
            Desktop = 1,
            Standard = 2,
            Enterprise = 3,
            Express = 4,
            Azure = 5
        }

        internal class ServerVersionInfo
        {
            public string Edition { get; set; }
            public string InstanceName { get; set; }
            public string ProductVersion { get; set; }
            public VersionName ProductVersionName { get; private set; }
            public EngineEdition EngineEdition { get; set; }
            public bool IsAzure => EngineEdition == EngineEdition.Azure;
            public string MachineName { get; set; }
            public string ProductLevel { get; set; }

            public void Initialize()
            {
                var firstPart = string.IsNullOrWhiteSpace(ProductVersion) ? "??" : ProductVersion.Split('.')[0];
                switch (firstPart)
                {
                    case "??":
                        ProductVersionName = VersionName.Invalid;
                        break;
                    case "12":
                        ProductVersionName = VersionName.V2014;
                        break;
                    case "11":
                        ProductVersionName = VersionName.V2012;
                        break;
                    case "10":
                        ProductVersionName = VersionName.V2008;
                        break;
                    case "9":
                        ProductVersionName = VersionName.V2005;
                        break;
                    case "8":
                        ProductVersionName = VersionName.V2000;
                        break;
                    case "7":
                        ProductVersionName = VersionName.V7;
                        break;
                    default:
                        ProductVersionName = VersionName.Other;
                        break;
                }
            }
        }

        private static ServerVersionInfo DetermineVersion(IDatabaseFactory factory)
        {
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

            try
            {
                var database = factory.GetDatabase();
                var version = database.Fetch<ServerVersionInfo>(sql).First();
                version.Initialize();
                return version;
            }
            catch (Exception e)
            {
                // can't ignore, really
                throw new Exception("Failed to determine Sql Server version (see inner exception).", e);
            }
        }

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

        public override string DeleteDefaultConstraint => "ALTER TABLE [{0}] DROP CONSTRAINT [DF_{0}_{1}]";


        public override string DropIndex => "DROP INDEX {0} ON {1}";

        public override string RenameColumn => "sp_rename '{0}.{1}', '{2}', 'COLUMN'";
    }
}