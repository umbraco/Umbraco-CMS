using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents an SqlSyntaxProvider for MySql
    /// </summary>
    [SqlSyntaxProviderAttribute("MySql.Data.MySqlClient")]
    public class MySqlSyntaxProvider : SqlSyntaxProviderBase<MySqlSyntaxProvider>
    {
        private readonly ILogger _logger;

        public MySqlSyntaxProvider(ILogger logger)
        {
            _logger = logger;
            DefaultStringLength = 255;
            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            AutoIncrementDefinition = "AUTO_INCREMENT";
            IntColumnDefinition = "int(11)";
            BoolColumnDefinition = "tinyint(1)";
            DateTimeColumnDefinition = "TIMESTAMP";
            TimeColumnDefinition = "time";
            DecimalColumnDefinition = "decimal(38,6)";
            GuidColumnDefinition = "char(36)";

            InitColumnTypeMap();

            DefaultValueFormat = "DEFAULT '{0}'";
        }

        public override IEnumerable<string> GetTablesInSchema(Database db)
        {
            List<string> list;
            try
            {
                //needs to be open to read the schema name
                db.OpenSharedConnection();

                var items =
                    db.Fetch<dynamic>(
                        "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @TableSchema",
                        new { TableSchema = db.Connection.Database });
                list = items.Select(x => x.TABLE_NAME).Cast<string>().ToList();
            }
            finally
            {
                db.CloseSharedConnection();
            }
            return list;
        }

        public override IEnumerable<ColumnInfo> GetColumnsInSchema(Database db)
        {
            List<ColumnInfo> list;
            try
            {
                //needs to be open to read the schema name
                db.OpenSharedConnection();

                var items =
                    db.Fetch<dynamic>(
                        "SELECT TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @TableSchema",
                        new { TableSchema = db.Connection.Database });
                list =
                    items.Select(
                        item =>
                        new ColumnInfo(item.TABLE_NAME, item.COLUMN_NAME, int.Parse(item.ORDINAL_POSITION.ToString()), item.COLUMN_DEFAULT ?? "",
                                       item.IS_NULLABLE, item.DATA_TYPE)).ToList();
            }
            finally
            {
                db.CloseSharedConnection();
            }
            return list;
        }

        public override IEnumerable<Tuple<string, string>> GetConstraintsPerTable(Database db)
        {
            List<Tuple<string, string>> list;
            try
            {
                //needs to be open to read the schema name
                db.OpenSharedConnection();

                //Does not include indexes and constraints are named differently
                var items =
                    db.Fetch<dynamic>(
                        "SELECT TABLE_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = @TableSchema",
                        new { TableSchema = db.Connection.Database });
                list = items.Select(item => new Tuple<string, string>(item.TABLE_NAME, item.CONSTRAINT_NAME)).ToList();
            }
            finally
            {
                db.CloseSharedConnection();
            }
            return list;
        }

        public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(Database db)
        {
            List<Tuple<string, string, string>> list;
            try
            {
                //needs to be open to read the schema name
                db.OpenSharedConnection();

                //Does not include indexes and constraints are named differently
                var items =
                    db.Fetch<dynamic>(
                        "SELECT TABLE_NAME, COLUMN_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = @TableSchema",
                        new { TableSchema = db.Connection.Database });
                list =
                    items.Select(
                        item =>
                        new Tuple<string, string, string>(item.TABLE_NAME, item.COLUMN_NAME, item.CONSTRAINT_NAME))
                         .ToList();
            }
            finally
            {
                db.CloseSharedConnection();
            }
            return list;
        }

        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(Database db)
        {
            List<Tuple<string, string, string, bool>> list;
            try
            {
                //needs to be open to read the schema name
                db.OpenSharedConnection();

                var indexes =
                db.Fetch<dynamic>(@"SELECT DISTINCT
    TABLE_NAME, INDEX_NAME, COLUMN_NAME, CASE NON_UNIQUE WHEN 1 THEN 0 ELSE 1 END AS `UNIQUE`
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = @TableSchema
AND INDEX_NAME <> COLUMN_NAME AND INDEX_NAME <> 'PRIMARY'
ORDER BY TABLE_NAME, INDEX_NAME",
                    new { TableSchema = db.Connection.Database });
                list =
                    indexes.Select(
                        item =>
                        new Tuple<string, string, string, bool>(item.TABLE_NAME, item.INDEX_NAME, item.COLUMN_NAME, item.UNIQUE == 1))
                         .ToList();
            }
            finally
            {
                db.CloseSharedConnection();
            }
            return list;
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            long result;
            try
            {
                //needs to be open to read the schema name
                db.OpenSharedConnection();

                result =
                    db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
                                           "WHERE TABLE_NAME = @TableName AND " +
                                           "TABLE_SCHEMA = @TableSchema",
                                           new { TableName = tableName, TableSchema = db.Connection.Database });

            }
            finally
            {
                db.CloseSharedConnection();
            }

            return result > 0;
        }

        public override bool SupportsClustered()
        {
            return true;
        }

        public override bool SupportsIdentityInsert()
        {
            return false;
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

        public override string GetSpecialDbType(SpecialDbTypes dbTypes)
        {
            if (dbTypes == SpecialDbTypes.NCHAR)
            {
                return "CHAR";
            }
            else if (dbTypes == SpecialDbTypes.NTEXT)
                return "LONGTEXT";

            return "NVARCHAR";
        }

        public override string Format(TableDefinition table)
        {
            string primaryKey = string.Empty;
            var columnDefinition = table.Columns.FirstOrDefault(x => x.IsPrimaryKey);
            if (columnDefinition != null)
            {
                string columns = string.IsNullOrEmpty(columnDefinition.PrimaryKeyColumns)
                                 ? GetQuotedColumnName(columnDefinition.Name)
                                 : string.Join(", ", columnDefinition.PrimaryKeyColumns
                                                                     .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(GetQuotedColumnName));

                primaryKey = string.Format(", \nPRIMARY KEY {0} ({1})", columnDefinition.IsIndexed ? "CLUSTERED" : "NONCLUSTERED", columns);
            }

            var statement = string.Format(CreateTable, GetQuotedTableName(table.Name), Format(table.Columns), primaryKey);

            return statement;
        }

        public override string Format(IndexDefinition index)
        {
            string name = string.IsNullOrEmpty(index.Name)
                                  ? string.Format("IX_{0}_{1}", index.TableName, index.ColumnName)
                                  : index.Name;

            string columns = index.Columns.Any()
                                 ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
                                 : GetQuotedColumnName(index.ColumnName);

            return string.Format(CreateIndex,
                                 GetQuotedName(name),
                                 GetQuotedTableName(index.TableName),
                                 columns);
        }

        public override string Format(ForeignKeyDefinition foreignKey)
        {
            return string.Format(CreateForeignKeyConstraint,
                                 GetQuotedTableName(foreignKey.ForeignTable),
                                 GetQuotedColumnName(foreignKey.ForeignColumns.First()),
                                 GetQuotedTableName(foreignKey.PrimaryTable),
                                 GetQuotedColumnName(foreignKey.PrimaryColumns.First()),
                                 FormatCascade("DELETE", foreignKey.OnDelete),
                                 FormatCascade("UPDATE", foreignKey.OnUpdate));
        }

        public override string FormatPrimaryKey(TableDefinition table)
        {
            return string.Empty;
        }

        protected override string FormatConstraint(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? AutoIncrementDefinition : string.Empty;
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue == null)
                return string.Empty;

            // see if this is for a system method
            if (column.DefaultValue is SystemMethods)
            {
                string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
                if (string.IsNullOrEmpty(method))
                    return string.Empty;

                return string.Format(DefaultValueFormat, method);
            }

            if (column.DefaultValue.ToString().ToLower().Equals("getdate()".ToLower()))
                return "DEFAULT CURRENT_TIMESTAMP";

            return string.Format(DefaultValueFormat, column.DefaultValue);
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            return string.Empty;
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
            get
            {
                return "ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT";
            }
        }

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }

        //CREATE TABLE {0} ({1}) ENGINE = INNODB versus CREATE TABLE {0} ({1}) ENGINE = MYISAM ?
        public override string CreateTable { get { return "CREATE TABLE {0} ({1}{2})"; } }

        public override string CreateIndex { get { return "CREATE INDEX {0} ON {1} ({2})"; } }

        public override string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD FOREIGN KEY ({1}) REFERENCES {2} ({3}){4}{5}"; } }

        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP {1} {2}"; } }

        public override string DropIndex { get { return "DROP INDEX {0} ON {1}"; } }

        public override string RenameColumn { get { return "ALTER TABLE {0} CHANGE {1} {2}"; } }

        public override bool? SupportsCaseInsensitiveQueries(Database db)
        {
            bool? supportsCaseInsensitiveQueries = null;

            try
            {
                db.OpenSharedConnection();
                // Need 4 @ signs as it is regarded as a parameter, @@ escapes it once, @@@@ escapes it twice
                var lowerCaseTableNames = db.Fetch<int>("SELECT @@@@Global.lower_case_table_names");

                if (lowerCaseTableNames.Any())
                    supportsCaseInsensitiveQueries = lowerCaseTableNames.First() == 1;
            }
            catch (Exception ex)
            {
                _logger.Error<MySqlSyntaxProvider>("Error querying for lower_case support", ex);
            }
            finally
            {
                db.CloseSharedConnection();
            }

            // Could return null, which means testing failed, 
            // add message to check with their hosting provider
            return supportsCaseInsensitiveQueries;
        }

        public override string EscapeString(string val)
        {
            return PetaPocoExtensions.EscapeAtSymbols(MySql.Data.MySqlClient.MySqlHelper.EscapeString(val));
        }
    }
}