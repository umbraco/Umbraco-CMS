using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NPoco;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Server.
    /// </summary>
    public class SqlServerSyntaxProvider : MicrosoftSqlSyntaxProviderBase<SqlServerSyntaxProvider>
    {
        internal ServerVersionInfo ServerVersion { get; private set; }

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
            V2016 = 7,
            V2017 = 8,
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

            public string Edition { get; }
            public string InstanceName { get; }
            public string ProductVersion { get; }
            public VersionName ProductVersionName { get; }
            public EngineEdition EngineEdition { get; }
            public bool IsAzure => EngineEdition == EngineEdition.Azure;
            public string MachineName { get; }
            public string ProductLevel { get; }
        }

        private static VersionName MapProductVersion(string productVersion)
        {
            var firstPart = string.IsNullOrWhiteSpace(productVersion) ? "??" : productVersion.Split('.')[0];
            switch (firstPart)
            {
                case "??":
                    return VersionName.Invalid;
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

        internal ServerVersionInfo GetSetVersion(string connectionString, string providerName, ILogger logger)
        {
            var factory = DbProviderFactories.GetFactory(providerName);
            var connection = factory.CreateConnection();

            if (connection == null)
                throw new InvalidOperationException($"Could not create a connection for provider \"{providerName}\".");

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
                => reader.IsDBNull(ordinal) ? defaultValue : reader.GetString(ordinal);

            int GetInt32(IDataReader reader, int ordinal, int defaultValue)
                => reader.IsDBNull(ordinal) ? defaultValue : reader.GetInt32(ordinal);

            connection.ConnectionString = connectionString;
            ServerVersionInfo version;
            using (connection)
            {
                try
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = sql;
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        // InstanceName can be NULL for the default instance
                        version = new ServerVersionInfo(
                            GetString(reader, 0, "Unknown"),
                            GetString(reader, 2, "(default)"),
                            GetString(reader, 3, string.Empty),
                            (EngineEdition) GetInt32(reader, 5, 0),
                            GetString(reader, 7, "DEFAULT"),
                            GetString(reader, 9, "Unknown"));
                    }
                    connection.Close();
                }
                catch (Exception e)
                {
                    logger.Error<UmbracoDatabaseFactory>(e, "Failed to detected SqlServer version.");
                    version = new ServerVersionInfo(); // all unknown
                }
            }

            return ServerVersion = version;
        }

        /// <summary>
        /// SQL Server stores default values assigned to columns as constraints, it also stores them with named values, this is the only
        /// server type that does this, therefore this method doesn't exist on any other syntax provider
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<string, string, string, string>> GetDefaultConstraintsPerColumn(IDatabase db)
        {
            var items = db.Fetch<dynamic>("SELECT TableName = t.Name, ColumnName = c.Name, dc.Name, dc.[Definition] FROM sys.tables t INNER JOIN sys.default_constraints dc ON t.object_id = dc.parent_object_id INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND c.column_id = dc.parent_column_id INNER JOIN sys.schemas as s on t.[schema_id] = s.[schema_id] WHERE s.name = (SELECT SCHEMA_NAME())");
            return items.Select(x => new Tuple<string, string, string, string>(x.TableName, x.ColumnName, x.Name, x.Definition));
        }

        public override IEnumerable<string> GetTablesInSchema(IDatabase db)
        {
            var items = db.Fetch<dynamic>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
            return items.Select(x => x.TABLE_NAME).Cast<string>().ToList();
        }

        public override IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db)
        {
            var items = db.Fetch<dynamic>("SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
            return
                items.Select(
                    item =>
                    new ColumnInfo(item.TABLE_NAME, item.COLUMN_NAME, item.ORDINAL_POSITION, item.COLUMN_DEFAULT,
                                   item.IS_NULLABLE, item.DATA_TYPE)).ToList();
        }

        /// <inheritdoc />
        public override IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db)
        {
            var items =
                db.Fetch<dynamic>(
                    "SELECT TABLE_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
            return items.Select(item => new Tuple<string, string>(item.TABLE_NAME, item.CONSTRAINT_NAME)).ToList();
        }

        /// <inheritdoc />
        public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db)
        {
            var items =
                db.Fetch<dynamic>(
                    "SELECT TABLE_NAME, COLUMN_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE WHERE TABLE_SCHEMA = (SELECT SCHEMA_NAME())");
            return items.Select(item => new Tuple<string, string, string>(item.TABLE_NAME, item.COLUMN_NAME, item.CONSTRAINT_NAME)).ToList();
        }

        /// <inheritdoc />
        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
        {
            var items =
                db.Fetch<dynamic>(
                    @"select T.name as TABLE_NAME, I.name as INDEX_NAME, AC.Name as COLUMN_NAME,
CASE WHEN I.is_unique_constraint = 1 OR  I.is_unique = 1 THEN 1 ELSE 0 END AS [UNIQUE]
from sys.tables as T inner join sys.indexes as I on T.[object_id] = I.[object_id]
   inner join sys.index_columns as IC on IC.[object_id] = I.[object_id] and IC.[index_id] = I.[index_id]
   inner join sys.all_columns as AC on IC.[object_id] = AC.[object_id] and IC.[column_id] = AC.[column_id]
   inner join sys.schemas as S on T.[schema_id] = S.[schema_id]
WHERE S.name = (SELECT SCHEMA_NAME()) AND I.is_primary_key = 0
order by T.name, I.name");
            return items.Select(item => new Tuple<string, string, string, bool>(item.TABLE_NAME, item.INDEX_NAME, item.COLUMN_NAME,
                item.UNIQUE == 1)).ToList();

        }

        /// <inheritdoc />
        public override bool TryGetDefaultConstraint(IDatabase db, string tableName, string columnName, out string constraintName)
        {
            constraintName = db.Fetch<string>(@"select con.[name] as [constraintName]
from sys.default_constraints con
join sys.columns col on con.object_id=col.default_object_id
join sys.tables tbl on col.object_id=tbl.object_id
where tbl.[name]=@0 and col.[name]=@1;", tableName, columnName)
                       .FirstOrDefault();
            return !constraintName.IsNullOrWhiteSpace();
        }

        public override bool DoesTableExist(IDatabase db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName AND TABLE_SCHEMA = (SELECT SCHEMA_NAME())",
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

        public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top)
        {
            return new Sql<ISqlContext>(sql.SqlContext, sql.SQL.Insert(sql.SQL.IndexOf(' '), " TOP " + top), sql.Arguments);
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

        public override string DeleteDefaultConstraint => "ALTER TABLE {0} DROP CONSTRAINT {2}";

        public override string DropIndex => "DROP INDEX {0} ON {1}";

        public override string RenameColumn => "sp_rename '{0}.{1}', '{2}', 'COLUMN'";
    }
}
