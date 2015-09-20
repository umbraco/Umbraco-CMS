using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents the Base Sql Syntax provider implementation.
    /// </summary>
    /// <remarks>
    /// All Sql Syntax provider implementations should derive from this abstract class.
    /// </remarks>
    /// <typeparam name="TSyntax"></typeparam>
    public abstract class SqlSyntaxProviderBase<TSyntax> : ISqlSyntaxProvider
        where TSyntax : ISqlSyntaxProvider
    {
        protected SqlSyntaxProviderBase()
        {
            ClauseOrder = new List<Func<ColumnDefinition, string>>
                              {
                                  FormatString,
                                  FormatType,
                                  FormatNullable,
                                  FormatConstraint,
                                  FormatDefaultValue,
                                  FormatPrimaryKey,
                                  FormatIdentity
                              };
        }

        public string GetWildcardPlaceholder()
        {
            return "%";
        }

        public string StringLengthNonUnicodeColumnDefinitionFormat = "VARCHAR({0})";
        public string StringLengthUnicodeColumnDefinitionFormat = "NVARCHAR({0})";

        public string DefaultValueFormat = "DEFAULT ({0})";
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

        protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; set; }

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

        public virtual string EscapeString(string val)
        {
            return PetaPocoExtensions.EscapeAtSymbols(val.Replace("'", "''"));
        }

        public virtual string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) = upper(@{1})", column, paramIndex);
        }

        public virtual string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) LIKE upper(@{1})", column, paramIndex);
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public virtual string GetStringColumnEqualComparison(string column, string value, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) = '{1}'", column, value.ToUpper());
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public virtual string GetStringColumnStartsWithComparison(string column, string value, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) LIKE '{1}%'", column, value.ToUpper());
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public virtual string GetStringColumnEndsWithComparison(string column, string value, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) LIKE '%{1}'", column, value.ToUpper());
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public virtual string GetStringColumnContainsComparison(string column, string value, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) LIKE '%{1}%'", column, value.ToUpper());
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public virtual string GetStringColumnWildcardComparison(string column, string value, TextColumnType columnType)
        {
            //use the 'upper' method to always ensure strings are matched without case sensitivity no matter what the db setting.
            return string.Format("upper({0}) LIKE '{1}'", column, value.ToUpper());
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

        public virtual bool? SupportsCaseInsensitiveQueries(Database db)
        {
            return true;
        }

        public virtual IEnumerable<string> GetTablesInSchema(Database db)
        {
            return new List<string>();
        }

        public virtual IEnumerable<ColumnInfo> GetColumnsInSchema(Database db)
        {
            return new List<ColumnInfo>();
        }

        public virtual IEnumerable<Tuple<string, string>> GetConstraintsPerTable(Database db)
        {
            return new List<Tuple<string, string>>();
        }

        public virtual IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(Database db)
        {
            return new List<Tuple<string, string, string>>();
        }

        public abstract IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(Database db);

        public virtual bool DoesTableExist(Database db, string tableName)
        {
            return false;
        }

        public virtual bool SupportsClustered()
        {
            return true;
        }

        public virtual bool SupportsIdentityInsert()
        {
            return true;
        }

        /// <summary>
        /// This is used ONLY if we need to format datetime without using SQL parameters (i.e. during migrations)
        /// </summary>
        /// <param name="date"></param>
        /// <param name="includeTime"></param>
        /// <returns></returns>
        /// <remarks>
        /// MSSQL has a DateTime standard that is unambiguous and works on all servers:
        /// YYYYMMDD HH:mm:ss
        /// </remarks>
        public virtual string FormatDateTime(DateTime date, bool includeTime = true)
        {
            // need CultureInfo.InvariantCulture because ":" here is the "time separator" and
            // may be converted to something else in different cultures (eg "." in DK).
            return date.ToString(includeTime ? "yyyyMMdd HH:mm:ss" : "yyyyMMdd", CultureInfo.InvariantCulture);
        }

        public virtual string Format(TableDefinition table)
        {
            var statement = string.Format(CreateTable, GetQuotedTableName(table.Name), Format(table.Columns));

            return statement;
        }

        public virtual List<string> Format(IEnumerable<IndexDefinition> indexes)
        {
            return indexes.Select(Format).ToList();
        }

        public virtual string Format(IndexDefinition index)
        {
            string name = string.IsNullOrEmpty(index.Name)
                                  ? string.Format("IX_{0}_{1}", index.TableName, index.ColumnName)
                                  : index.Name;

            string columns = index.Columns.Any()
                                 ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
                                 : GetQuotedColumnName(index.ColumnName);

            return string.Format(CreateIndex, GetIndexType(index.IndexType), " ", GetQuotedName(name),
                                 GetQuotedTableName(index.TableName), columns);
        }

        public virtual List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys)
        {
            return foreignKeys.Select(Format).ToList();
        }

        public virtual string Format(ForeignKeyDefinition foreignKey)
        {
            string constraintName = string.IsNullOrEmpty(foreignKey.Name)
                                        ? string.Format("FK_{0}_{1}_{2}", foreignKey.ForeignTable, foreignKey.PrimaryTable, foreignKey.PrimaryColumns.First())
                                        : foreignKey.Name;

            return string.Format(CreateForeignKeyConstraint,
                                 GetQuotedTableName(foreignKey.ForeignTable),
                                 GetQuotedName(constraintName),
                                 GetQuotedColumnName(foreignKey.ForeignColumns.First()),
                                 GetQuotedTableName(foreignKey.PrimaryTable),
                                 GetQuotedColumnName(foreignKey.PrimaryColumns.First()),
                                 FormatCascade("DELETE", foreignKey.OnDelete), 
                                 FormatCascade("UPDATE", foreignKey.OnUpdate));
        }

        public virtual string Format(IEnumerable<ColumnDefinition> columns)
        {
            var sb = new StringBuilder();
            foreach (var column in columns)
            {
                sb.Append(Format(column) +",\n");
            }
            return sb.ToString().TrimEnd(",\n");
        }

        public virtual string Format(ColumnDefinition column)
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

        public virtual string FormatPrimaryKey(TableDefinition table)
        {
            var columnDefinition = table.Columns.FirstOrDefault(x => x.IsPrimaryKey);
            if (columnDefinition == null)
                return string.Empty;

            string constraintName = string.IsNullOrEmpty(columnDefinition.PrimaryKeyName)
                                        ? string.Format("PK_{0}", table.Name)
                                        : columnDefinition.PrimaryKeyName;

            string columns = string.IsNullOrEmpty(columnDefinition.PrimaryKeyColumns)
                                 ? GetQuotedColumnName(columnDefinition.Name)
                                 : string.Join(", ", columnDefinition.PrimaryKeyColumns
                                                                     .Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(GetQuotedColumnName));

            string primaryKeyPart = string.Concat("PRIMARY KEY", columnDefinition.IsIndexed ? " CLUSTERED" : " NONCLUSTERED");

            return string.Format(CreateConstraint,
                                 GetQuotedTableName(table.Name),
                                 GetQuotedName(constraintName),
                                 primaryKeyPart,
                                 columns);
        }

        public virtual string FormatColumnRename(string tableName, string oldName, string newName)
        {
            return string.Format(RenameColumn,
                                 GetQuotedTableName(tableName),
                                 GetQuotedColumnName(oldName),
                                 GetQuotedColumnName(newName));
        }

        public virtual string FormatTableRename(string oldName, string newName)
        {
            return string.Format(RenameTable, GetQuotedTableName(oldName), GetQuotedTableName(newName));
        }

        protected virtual string FormatCascade(string onWhat, Rule rule)
        {
            string action = "NO ACTION";
            switch (rule)
            {
                case Rule.None:
                    return "";
                case Rule.Cascade:
                    action = "CASCADE";
                    break;
                case Rule.SetNull:
                    action = "SET NULL";
                    break;
                case Rule.SetDefault:
                    action = "SET DEFAULT";
                    break;
            }

            return string.Format(" ON {0} {1}", onWhat, action);
        }

        protected virtual string FormatString(ColumnDefinition column)
        {
            return GetQuotedColumnName(column.Name);
        }

        protected virtual string FormatType(ColumnDefinition column)
        {
            if (column.Type.HasValue == false && string.IsNullOrEmpty(column.CustomType) == false)
                return column.CustomType;

            if (column.HasSpecialDbType)
            {
                if (column.Size != default(int))
                {
                    return string.Format("{0}({1})",
                                         GetSpecialDbType(column.DbType),
                                         column.Size);
                }

                return GetSpecialDbType(column.DbType);
            }

            Type type = column.Type.HasValue 
                ? DbTypeMap.ColumnDbTypeMap.First(x => x.Value == column.Type.Value).Key
                : column.PropertyType;

            if (type == typeof (string))
            {
                var valueOrDefault = column.Size != default(int) ? column.Size : DefaultStringLength;
                return string.Format(StringLengthColumnDefinitionFormat, valueOrDefault);
            }

            string definition = DbTypeMap.ColumnTypeMap.First(x => x.Key == type).Value;
            string dbTypeDefinition = column.Size != default(int)
                                          ? string.Format("{0}({1})", definition, column.Size)
                                          : definition;
            //NOTE Percision is left out
            return dbTypeDefinition;
        }

        protected virtual string FormatNullable(ColumnDefinition column)
        {
            return column.IsNullable ? "NULL" : "NOT NULL";
        }

        protected virtual string FormatConstraint(ColumnDefinition column)
        {
            if (string.IsNullOrEmpty(column.ConstraintName) && column.DefaultValue == null)
                return string.Empty;

            return string.Format("CONSTRAINT {0}",
                                 string.IsNullOrEmpty(column.ConstraintName)
                                     ? GetQuotedName(string.Format("DF_{0}_{1}", column.TableName, column.Name))
                                     : column.ConstraintName);
        }

        protected virtual string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue == null)
                return string.Empty;

            //hack - probably not needed with latest changes
            if (column.DefaultValue.ToString().ToLower().Equals("getdate()".ToLower()))
                column.DefaultValue = SystemMethods.CurrentDateTime;

            // see if this is for a system method
            if (column.DefaultValue is SystemMethods)
            {
                string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
                if (string.IsNullOrEmpty(method))
                    return string.Empty;

                return string.Format(DefaultValueFormat, method);
            }

            return string.Format(DefaultValueFormat, GetQuotedValue(column.DefaultValue.ToString()));
        }

        protected virtual string FormatPrimaryKey(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected abstract string FormatSystemMethods(SystemMethods systemMethod);

        protected abstract string FormatIdentity(ColumnDefinition column);

        public virtual string DeleteDefaultConstraint
        {
            get
            {
                throw new NotSupportedException("Default constraints are not supported");
            }
        }

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
        public virtual string TruncateTable { get { return "TRUNCATE TABLE {0}"; } }

        public virtual string CreateConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} {2} ({3})"; } }
        public virtual string DeleteConstraint { get { return "ALTER TABLE {0} DROP CONSTRAINT {1}"; } }
        public virtual string CreateForeignKeyConstraint { get { return "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4}){5}{6}"; } }
    }
}