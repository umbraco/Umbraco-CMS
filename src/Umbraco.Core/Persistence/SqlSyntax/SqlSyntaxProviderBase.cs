using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.SqlSyntax.ModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    public interface ISqlSyntaxProvider
    {
        string GetQuotedTableName(string tableName);
        string GetQuotedColumnName(string columnName);
        string GetQuotedName(string name);
        bool DoesTableExist(Database db, string tableName);
        string ToCreateTableStatement(TableDefinition tableDefinition);
        List<string> ToCreateForeignKeyStatements(TableDefinition tableDefinition);
        List<string> ToCreateIndexStatements(TableDefinition tableDefinition);
        DbType GetColumnDbType(Type valueType);
        string GetIndexType(IndexTypes indexTypes);
        string GetColumnDefinition(ColumnDefinition column, string tableName);
        string GetPrimaryKeyStatement(ColumnDefinition column, string tableName);
        string ToCreatePrimaryKeyStatement(TableDefinition table);
        string GetSpecialDbType(SpecialDbTypes dbTypes);
        string GetConstraintDefinition(ColumnDefinition column, string tableName);
    }

    internal abstract class SqlSyntaxProviderBase<TSyntax> : ISqlSyntaxProvider
        where TSyntax : ISqlSyntaxProvider
    {
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
    }
}