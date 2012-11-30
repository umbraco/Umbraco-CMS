using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Migrations.Model;
using ColumnDefinition = Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions.ColumnDefinition;
using TableDefinition = Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions.TableDefinition;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents the Base Sql Syntax provider implementation.
    /// </summary>
    /// <remarks>
    /// All Sql Syntax provider implementations should derive from this abstract class.
    /// </remarks>
    /// <typeparam name="TSyntax"></typeparam>
    internal abstract class SqlSyntaxProviderBase<TSyntax> : ISqlSyntaxProvider
        where TSyntax : ISqlSyntaxProvider
    {
        protected SqlSyntaxProviderBase()
        {
            ClauseOrder = new List<Func<Migrations.Model.ColumnDefinition, string>>
                              {
                                  FormatString,
                                  FormatType,
                                  FormatNullable,
                                  FormatDefaultValue,
                                  FormatPrimaryKey,
                                  FormatIdentity
                              };
        }

        public string StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR({0})";
        public string StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR({0})";

        public string DefaultValueFormat = " DEFAULT ({0})";
        public int DefaultStringLength = 255;

        //Set by Constructor
        public string StringColumnDefinition;
        public string StringLengthColumnDefinitionFormat;

        public string AutoIncrementDefinition = "AUTOINCREMENT";
        public string IntColumnDefinition = "INTEGER";
        public string LongColumnDefinition = "BIGINT";
        public string GuidColumnDefinition = "GUID";
        public string BoolColumnDefinition = "BOOL";
        public string RealColumnDefinition = "DOUBLE";
        public string DecimalColumnDefinition = "DECIMAL";
        public string BlobColumnDefinition = "BLOB";
        public string DateTimeColumnDefinition = "DATETIME";
        public string TimeColumnDefinition = "DATETIME";

        protected IList<Func<Migrations.Model.ColumnDefinition, string>> ClauseOrder { get; set; }

        protected DbTypes<TSyntax> DbTypeMap = new DbTypes<TSyntax>();
        protected void InitColumnTypeMap()
        {
            DbTypeMap.Set<string>(DbType.String, StringColumnDefinition);
            DbTypeMap.Set<char>(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set<char?>(DbType.StringFixedLength, StringColumnDefinition);
            DbTypeMap.Set<char[]>(DbType.String, StringColumnDefinition);
            DbTypeMap.Set<bool>(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set<bool?>(DbType.Boolean, BoolColumnDefinition);
            DbTypeMap.Set<Guid>(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set<Guid?>(DbType.Guid, GuidColumnDefinition);
            DbTypeMap.Set<DateTime>(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set<DateTime?>(DbType.DateTime, DateTimeColumnDefinition);
            DbTypeMap.Set<TimeSpan>(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set<TimeSpan?>(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set<DateTimeOffset>(DbType.Time, TimeColumnDefinition);
            DbTypeMap.Set<DateTimeOffset?>(DbType.Time, TimeColumnDefinition);

            DbTypeMap.Set<byte>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<byte?>(DbType.Byte, IntColumnDefinition);
            DbTypeMap.Set<sbyte>(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set<sbyte?>(DbType.SByte, IntColumnDefinition);
            DbTypeMap.Set<short>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<short?>(DbType.Int16, IntColumnDefinition);
            DbTypeMap.Set<ushort>(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set<ushort?>(DbType.UInt16, IntColumnDefinition);
            DbTypeMap.Set<int>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<int?>(DbType.Int32, IntColumnDefinition);
            DbTypeMap.Set<uint>(DbType.UInt32, IntColumnDefinition);
            DbTypeMap.Set<uint?>(DbType.UInt32, IntColumnDefinition);

            DbTypeMap.Set<long>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<long?>(DbType.Int64, LongColumnDefinition);
            DbTypeMap.Set<ulong>(DbType.UInt64, LongColumnDefinition);
            DbTypeMap.Set<ulong?>(DbType.UInt64, LongColumnDefinition);

            DbTypeMap.Set<float>(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set<float?>(DbType.Single, RealColumnDefinition);
            DbTypeMap.Set<double>(DbType.Double, RealColumnDefinition);
            DbTypeMap.Set<double?>(DbType.Double, RealColumnDefinition);

            DbTypeMap.Set<decimal>(DbType.Decimal, DecimalColumnDefinition);
            DbTypeMap.Set<decimal?>(DbType.Decimal, DecimalColumnDefinition);

            DbTypeMap.Set<byte[]>(DbType.Binary, BlobColumnDefinition);
        }

        public virtual string GetQuotedTableName(string tableName)
        {
            return string.Format("\"{0}\"", tableName);
        }

        public virtual string GetQuotedColumnName(string columnName)
        {
            return string.Format("\"{0}\"", columnName);
        }

        public virtual string GetQuotedName(string name)
        {
            return string.Format("\"{0}\"", name);
        }

        public virtual string GetQuotedValue(string value)
        {
            return string.Format("'{0}'", value);
        }

        public virtual string GetIndexType(IndexTypes indexTypes)
        {
            string indexType;

            if (indexTypes == IndexTypes.Clustered)
            {
                indexType = "CLUSTERED";
            }
            else
            {
                indexType = indexTypes == IndexTypes.NonClustered
                    ? "NONCLUSTERED"
                    : "UNIQUE NONCLUSTERED";
            }
            return indexType;
        }

        public virtual string GetSpecialDbType(SpecialDbTypes dbTypes)
        {
            if (dbTypes == SpecialDbTypes.NCHAR)
            {
                return "NCHAR";
            }
            else if (dbTypes == SpecialDbTypes.NTEXT)
                return "NTEXT";

            return "NVARCHAR";
        }

        public virtual string GetColumnDefinition(ColumnDefinition column, string tableName)
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
                sql.Append(" NOT NULL ").Append(AutoIncrementDefinition);
            }
            else
            {
                sql.Append(column.IsNullable ? " NULL" : " NOT NULL");
            }

            if(column.HasConstraint)
            {
                sql.Append(GetConstraintDefinition(column, tableName));
            }

            return sql.ToString();
        }

        public virtual string GetConstraintDefinition(ColumnDefinition column, string tableName)
        {
            var sql = new StringBuilder();
            sql.AppendFormat(" CONSTRAINT {0}",
                                 string.IsNullOrEmpty(column.ConstraintName)
                                     ? GetQuotedName(string.Format("DF_{0}_{1}", tableName, column.ColumnName))
                                     : column.ConstraintName);

            string value = column.PropertyType == typeof (string)
                               ? GetQuotedValue(column.ConstraintDefaultValue)
                               : column.ConstraintDefaultValue;

            sql.AppendFormat(DefaultValueFormat, value);
            return sql.ToString();
        }

        public virtual string GetPrimaryKeyStatement(ColumnDefinition column, string tableName)
        {
            string constraintName = string.IsNullOrEmpty(column.PrimaryKeyName)
                                        ? string.Format("PK_{0}", tableName)
                                        : column.PrimaryKeyName;

            string columns = string.IsNullOrEmpty(column.PrimaryKeyColumns)
                                 ? GetQuotedColumnName(column.ColumnName)
                                 : column.PrimaryKeyColumns;

            string sql = string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY {2} ({3}); \n",
                                       GetQuotedTableName(tableName),
                                       GetQuotedName(constraintName),
                                       column.IsPrimaryKeyClustered ? "CLUSTERED" : "NONCLUSTERED",
                                       columns);
            return sql;
        }

        public virtual string ToCreateTableStatement(TableDefinition table)
        {
            var columns = new StringBuilder();

            foreach (var column in table.ColumnDefinitions)
            {
                columns.Append(GetColumnDefinition(column, table.TableName) + ", \n");
            }

            string sql = string.Format("CREATE TABLE {0} \n(\n {1} \n); \n",
                                       table.TableName,
                                       columns.ToString().TrimEnd(", \n".ToCharArray()));

            return sql;
        }

        public virtual string ToCreatePrimaryKeyStatement(TableDefinition table)
        {
            var columnDefinition = table.ColumnDefinitions.FirstOrDefault(x => x.IsPrimaryKey);
            if (columnDefinition == null)
                return string.Empty;

            var sql = GetPrimaryKeyStatement(columnDefinition, table.TableName);
            return sql;
        }

        public virtual List<string> ToCreateForeignKeyStatements(TableDefinition table)
        {
            var foreignKeys = new List<string>();

            foreach (var key in table.ForeignKeyDefinitions)
            {
                string constraintName = string.IsNullOrEmpty(key.ConstraintName)
                                        ? string.Format("FK_{0}_{1}_{2}", table.TableName, key.ReferencedTableName, key.ReferencedColumnName)
                                        : key.ConstraintName;

                foreignKeys.Add(string.Format("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}); \n",
                                         GetQuotedTableName(table.TableName),
                                         GetQuotedName(constraintName),
                                         GetQuotedColumnName(key.ColumnName),
                                         GetQuotedTableName(key.ReferencedTableName),
                                         GetQuotedColumnName(key.ReferencedColumnName)));
            }

            return foreignKeys;
        }

        public virtual List<string> ToCreateIndexStatements(TableDefinition table)
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

                indexes.Add(string.Format("CREATE {0} INDEX {1} ON {2} ({3}); \n",
                                     GetIndexType(index.IndexType),
                                     GetQuotedName(name),
                                     GetQuotedTableName(table.TableName),
                                     columns));
            }
            return indexes;
        }

        public virtual List<string> ToAlterIdentitySeedStatements(TableDefinition table)
        {
            var seeds = new List<string>();

            foreach (var definition in table.ColumnDefinitions)
            {
                if (definition.PrimaryKeySeeding > 0)
                {
                    seeds.Add(string.Format("ALTER TABLE {0} ALTER COLUMN {1} IDENTITY({2},1); \n",
                                            GetQuotedTableName(table.TableName),
                                            GetQuotedColumnName(definition.ColumnName),
                                            definition.PrimaryKeySeeding));
                }
            }

            return seeds;
        }

        public virtual bool DoesTableExist(Database db, string tableName)
        {
            return false;
        }

        public virtual DbType GetColumnDbType(Type valueType)
        {
            if (valueType.IsEnum)
                return DbTypeMap.ColumnDbTypeMap[typeof(string)];

            return DbTypeMap.ColumnDbTypeMap[valueType];
        }

        public virtual string Format(Migrations.Model.ColumnDefinition column)
        {
            var clauses = new List<string>();

            foreach (var action in ClauseOrder)
            {
                string clause = action(column);
                if (!string.IsNullOrEmpty(clause))
                    clauses.Add(clause);
            }

            return string.Join(" ", clauses.ToArray());
        }

        public virtual string FormatString(Migrations.Model.ColumnDefinition column)
        {
            return GetQuotedColumnName(column.Name);
        }

        protected virtual string FormatType(Migrations.Model.ColumnDefinition column)
        {
            if (!column.Type.HasValue)
                return column.CustomType;

            var dbType = DbTypeMap.ColumnDbTypeMap.First(x => x.Value == column.Type.Value).Key;
            var definition = DbTypeMap.ColumnTypeMap.First(x => x.Key == dbType).Value;

            string dbTypeDefinition = column.Size != default(int)
                                          ? string.Format("{0}({1})", definition, column.Size)
                                          : definition;
            //NOTE Percision is left out
            return dbTypeDefinition;
        }

        protected virtual string FormatNullable(Migrations.Model.ColumnDefinition column)
        {
            return !column.IsNullable ? "NOT NULL" : string.Empty;
        }

        protected virtual string FormatDefaultValue(Migrations.Model.ColumnDefinition column)
        {
            if (column.DefaultValue == null)
                return string.Empty;

            // see if this is for a system method
            if (column.DefaultValue is SystemMethods)
            {
                string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
                if (string.IsNullOrEmpty(method))
                    return string.Empty;

                return "DEFAULT " + method;
            }

            return "DEFAULT " + GetQuotedValue(column.DefaultValue.ToString());
        }

        protected virtual string FormatPrimaryKey(Migrations.Model.ColumnDefinition column)
        {
            return string.Empty;
        }

        protected abstract string FormatSystemMethods(SystemMethods systemMethod);

        protected abstract string FormatIdentity(Migrations.Model.ColumnDefinition column);

        public virtual string CreateTable { get { return "CREATE TABLE {0} ({1})"; } }
        public virtual string DropTable { get { return "DROP TABLE {0}"; } }

        public virtual string AddColumn { get { return "ALTER TABLE {0} ADD COLUMN {1}"; } }
        public virtual string DropColumn { get { return "ALTER TABLE {0} DROP COLUMN {1}"; } }
        public virtual string AlterColumn { get { return "ALTER TABLE {0} ALTER COLUMN {1}"; } }
        public virtual string RenameColumn { get { return "ALTER TABLE {0} RENAME COLUMN {1} TO {2}"; } }

        public virtual string RenameTable { get { return "RENAME TABLE {0} TO {1}"; } }

        public virtual string CreateSchema { get { return "CREATE SCHEMA {0}"; } }
        public virtual string AlterSchema { get { return "ALTER SCHEMA {0} TRANSFER {1}.{2}"; } }
        public virtual string DropSchema { get { return "DROP SCHEMA {0}"; } }

        public virtual string CreateIndex { get { return "CREATE {0}{1}INDEX {2} ON {3} ({4})"; } }
        public virtual string DropIndex { get { return "DROP INDEX {0}"; } }

        public virtual string InsertData { get { return "INSERT INTO {0} ({1}) VALUES ({2})"; } }
        public virtual string UpdateData { get { return "UPDATE {0} SET {1} WHERE {2}"; } }
        public virtual string DeleteData { get { return "DELETE FROM {0} WHERE {1}"; } }

        public virtual string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})"; } }
        public virtual string DeleteConstraint { get { return "ALTER TABLE {0} DROP CONSTRAINT {1}"; } }
        public virtual string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}"; } }
    }
}