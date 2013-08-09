using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Static class that provides simple access to the Sqlite SqlSyntax Provider
    /// </summary>
    internal static class SqliteSyntax
    {
        public static ISqlSyntaxProvider Provider { get { return new SqliteSyntaxProvider(); } }
    }

    /// <summary>
    /// Represents an SqlSyntaxProvider for Sqlite
    /// </summary>
    [SqlSyntaxProviderAttribute("System.Data.SQLite")]
    public class SqliteSyntaxProvider : SqlSyntaxProviderBase<SqliteSyntaxProvider>
    {
        public SqliteSyntaxProvider()
        {
            ClauseOrder = new List<Func<ColumnDefinition, string>>
                              {
                                  FormatString,
                                  FormatType,
                                  FormatConstraint,
                                  FormatDefaultValue,
                                  FormatPrimaryKey,
                                  FormatIdentity,
                                  FormatNullable
                              };

            StringLengthColumnDefinitionFormat = StringLengthUnicodeColumnDefinitionFormat;
            StringColumnDefinition = string.Format(StringLengthColumnDefinitionFormat, DefaultStringLength);

            DateTimeColumnDefinition = StringColumnDefinition;
            BoolColumnDefinition = IntColumnDefinition;
            GuidColumnDefinition = "CHAR(32)";

            InitColumnTypeMap();
        }

        public override bool SupportsClustered()
        {
            return false;
        }

        public override bool SupportsIdentityInsert()
        {
            return false;
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name = @TableName",
                                       new { TableName = tableName });

            return result > 0;
        }

        public override string GetQuotedTableName(string tableName)
        {
            return string.Format("\"{0}\"", tableName);
        }

        public override string GetQuotedColumnName(string columnName)
        {
            return string.Format("\"{0}\"", columnName);
        }

        public override string GetIndexType(IndexTypes indexTypes)
        {
            //NOTE Sqlite doesn't support index types
            return string.Empty;
        }

        public override string Format(TableDefinition table)
        {
            var columns = Format(table.Columns);
            var tablesInnerCreateStatement = columns;

            var primaryKeyColumn = table.Columns.SingleOrDefault(x => x.IsPrimaryKey);
            if (primaryKeyColumn != null)
                tablesInnerCreateStatement = string.Concat(tablesInnerCreateStatement, ",\n",
                    string.Format("CONSTRAINT {0} PRIMARY KEY ({1})", primaryKeyColumn.PrimaryKeyName,
                    string.IsNullOrEmpty(primaryKeyColumn.PrimaryKeyColumns) ? primaryKeyColumn.Name : primaryKeyColumn.PrimaryKeyColumns));

            var foreignKeys = FormatForeignKeys(table.ForeignKeys);
            if (foreignKeys.Any())
                tablesInnerCreateStatement = string.Concat(tablesInnerCreateStatement, ",\n", string.Join(",\n", foreignKeys));

            var statement = string.Format(CreateTable, GetQuotedTableName(table.Name), tablesInnerCreateStatement);

            return statement;
        }

        public override string Format(ForeignKeyDefinition foreignKey)
        {
            return string.Empty;
        }

        public override string Format(IndexDefinition index)
        {
            string name = string.IsNullOrEmpty(index.Name)
                                  ? string.Format("IX_{0}_{1}", index.TableName, index.ColumnName)
                                  : index.Name;

            string columns = index.Columns.Any()
                                 ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
                                 : GetQuotedColumnName(index.ColumnName);

            return string.Format(CreateIndex, GetIndexType(index.IndexType), "", name,
                                 GetQuotedTableName(index.TableName), columns);
        }

        protected override string FormatNullable(ColumnDefinition column)
        {
            return column.IsNullable ? "NULL" : "NOT NULL";
        }

        protected override string FormatConstraint(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            /*if(column.IsPrimaryKey)
                return "PRIMARY KEY";*/

            return string.Empty;
        }

        protected List<string> FormatForeignKeys(IEnumerable<ForeignKeyDefinition> foreignKeys)
        {
            return foreignKeys.Select(FormatForeignKey).Where(formatted => string.IsNullOrEmpty(formatted) == false).ToList();
        }

        protected string FormatForeignKey(ForeignKeyDefinition foreignKey)
        {
            return string.Format("FOREIGN KEY({0}) REFERENCES {1}({2})", foreignKey.ForeignColumns.First(),
                foreignKey.PrimaryTable, foreignKey.PrimaryColumns.First());
        }

        public override string FormatPrimaryKey(TableDefinition table)
        {
            return string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            return string.Empty;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            /*if (column.IsIdentity)
                return "AUTOINCREMENT";*/

            return string.Empty;
        }
    }
}