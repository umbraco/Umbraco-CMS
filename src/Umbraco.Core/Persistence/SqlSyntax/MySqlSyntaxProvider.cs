using System.Collections.Generic;
using System.Text;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations.Model;
using ColumnDefinition = Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions.ColumnDefinition;
using TableDefinition = Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions.TableDefinition;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the MySql SqlSyntax Providers singleton
    /// </summary>
    internal static class MySqlSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return MySqlSyntaxProvider.Instance; } }
    }

    /// <summary>
    /// Represents an SqlSyntaxProvider for MySql
    /// </summary>
    internal class MySqlSyntaxProvider : SqlSyntaxProviderBase<MySqlSyntaxProvider>
    {
        public static MySqlSyntaxProvider Instance = new MySqlSyntaxProvider();

        private MySqlSyntaxProvider()
        {
            DefaultStringLength = 255;
            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            AutoIncrementDefinition = "AUTO_INCREMENT";
            IntColumnDefinition = "int(11)";
            BoolColumnDefinition = "tinyint(1)";
            TimeColumnDefinition = "time";
            DecimalColumnDefinition = "decimal(38,6)";
            GuidColumnDefinition = "char(32)";
            
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

        public override string GetConstraintDefinition(ColumnDefinition column, string tableName)
        {
            var sql = new StringBuilder();

            if (column.ConstraintDefaultValue.Equals("getdate()"))
            {
                sql.Append(" DEFAULT CURRENT_TIMESTAMP");
            }
            else
            {
                sql.AppendFormat(DefaultValueFormat, column.ConstraintDefaultValue);
            }

            return sql.ToString();
        }

        public override string GetColumnDefinition(ColumnDefinition column, string tableName)
        {
            string dbTypeDefinition;
            if (column.HasSpecialDbType)
            {
                if (column.DbTypeLength.HasValue)
                {
                    dbTypeDefinition = string.Format("{0}({1})",
                                                     GetSpecialDbType(column.DbType),
                                                     column.DbTypeLength.Value);
                }
                else
                {
                    dbTypeDefinition = GetSpecialDbType(column.DbType);
                }
            }
            else if (column.PropertyType == typeof(string))
            {
                dbTypeDefinition = string.Format(StringLengthColumnDefinitionFormat, column.DbTypeLength.GetValueOrDefault(DefaultStringLength));
            }
            else
            {
                if (!DbTypeMap.ColumnTypeMap.TryGetValue(column.PropertyType, out dbTypeDefinition))
                {
                    dbTypeDefinition = "";
                }
            }

            var sql = new StringBuilder();
            sql.AppendFormat("{0} {1}", GetQuotedColumnName(column.ColumnName), dbTypeDefinition);

            if (column.IsPrimaryKeyIdentityColumn)
            {
                sql.Append(" NOT NULL PRIMARY KEY ").Append(AutoIncrementDefinition);
            }
            else
            {
                sql.Append(column.IsNullable ? " NULL" : " NOT NULL");
            }

            if (column.HasConstraint)
            {
                sql.Append(GetConstraintDefinition(column, tableName));
                sql = sql.Replace("DATETIME", "TIMESTAMP");
            }

            return sql.ToString();
        }

        public override string GetPrimaryKeyStatement(ColumnDefinition column, string tableName)
        {
            return string.Empty;
        }

        public override List<string> ToCreateIndexStatements(TableDefinition table)
        {
            var indexes = new List<string>();

            foreach (var index in table.IndexDefinitions)
            {
                string name = string.IsNullOrEmpty(index.IndexName)
                                  ? string.Format("IX_{0}_{1}", table.TableName, index.IndexForColumn)
                                  : index.IndexName;

                string columns = string.IsNullOrEmpty(index.ColumnNames)
                                     ? GetQuotedColumnName(index.IndexForColumn)
                                     : index.ColumnNames;

                indexes.Add(string.Format("CREATE INDEX {0} ON {1} ({2}); \n",
                                     GetQuotedName(name),
                                     GetQuotedTableName(table.TableName),
                                     columns));
            }
            return indexes;
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            db.OpenSharedConnection();
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES " +
                "WHERE TABLE_NAME = @TableName AND " +
                "TABLE_SCHEMA = @TableSchema", new { TableName = tableName, TableSchema = db.Connection.Database });

            return result > 0;
        }

        protected override string FormatIdentity(Migrations.Model.ColumnDefinition column)
        {
            return column.IsIdentity ? AutoIncrementDefinition : string.Empty;
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

        public override string AlterColumn { get { return "ALTER TABLE {0} MODIFY COLUMN {1}"; } }

        public override string DeleteConstraint { get { return "ALTER TABLE {0} DROP {1}{2}"; } }

        public override string CreateTable { get { return "CREATE TABLE {0} ({1}) ENGINE = INNODB"; } }
    }
}