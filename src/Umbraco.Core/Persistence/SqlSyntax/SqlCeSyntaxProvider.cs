using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Represents an SqlSyntaxProvider for Sql Ce
    /// </summary>
    [SqlSyntaxProviderAttribute("System.Data.SqlServerCe.4.0")]
    public class SqlCeSyntaxProvider : MicrosoftSqlSyntaxProviderBase<SqlCeSyntaxProvider>
    {
        public SqlCeSyntaxProvider()
        {
            
        }

        public override bool SupportsClustered()
        {
            return false;
        }

        /// <summary>
        /// SqlCe doesn't support the Truncate Table syntax, so we just have to do a DELETE FROM which is slower but we have no choice.
        /// </summary>
        public override string TruncateTable
        {
            get { return "DELETE FROM {0}"; }
        }

        public override string GetIndexType(IndexTypes indexTypes)
        {
            string indexType;
            //NOTE Sql Ce doesn't support clustered indexes
            if (indexTypes == IndexTypes.Clustered)
            {
                indexType = "NONCLUSTERED";
            }
            else
            {
                indexType = indexTypes == IndexTypes.NonClustered
                    ? "NONCLUSTERED"
                    : "UNIQUE NONCLUSTERED";
            }
            return indexType;
        }

        [Obsolete("Use the overload with the parameter index instead")]
        public override string GetStringColumnEqualComparison(string column, string value, TextColumnType columnType)
        {
            switch (columnType)
            {
                case TextColumnType.NVarchar:
                    return base.GetStringColumnEqualComparison(column, value, columnType);
                case TextColumnType.NText:
                    //MSSQL doesn't allow for = comparison with NText columns but allows this syntax
                    return string.Format("{0} LIKE '{1}'", column, value);
                default:
                    throw new ArgumentOutOfRangeException("columnType");
            }   
        }

        

        public override string FormatColumnRename(string tableName, string oldName, string newName)
        {
            //NOTE Sql CE doesn't support renaming a column, so a new column needs to be created, then copy data and finally remove old column
            //This assumes that the new column has been created, and that the old column will be deleted after this statement has run.
            //http://stackoverflow.com/questions/3967353/microsoft-sql-compact-edition-rename-column

            return string.Format("UPDATE {0} SET {1} = {2}", tableName, newName, oldName);
        }

        public override string FormatTableRename(string oldName, string newName)
        {
            return string.Format(RenameTable, oldName, newName);
        }

        public override string FormatPrimaryKey(TableDefinition table)
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
                                                                     .Split(new[]{',', ' '}, StringSplitOptions.RemoveEmptyEntries)
                                                                     .Select(GetQuotedColumnName));

            return string.Format(CreateConstraint,
                                 GetQuotedTableName(table.Name),
                                 GetQuotedName(constraintName),
                                 "PRIMARY KEY",
                                 columns);
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
            var items = db.Fetch<dynamic>("SELECT TABLE_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS");
            var indexItems = db.Fetch<dynamic>("SELECT TABLE_NAME, INDEX_NAME FROM INFORMATION_SCHEMA.INDEXES");
            return
                items.Select(item => new Tuple<string, string>(item.TABLE_NAME, item.CONSTRAINT_NAME))
                     .Union(
                         indexItems.Select(
                             indexItem => new Tuple<string, string>(indexItem.TABLE_NAME, indexItem.INDEX_NAME)))
                     .ToList();
        }

        public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(Database db)
        {
            var items =
                db.Fetch<dynamic>(
                    "SELECT CONSTRAINT_NAME, TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE");
            var indexItems = db.Fetch<dynamic>("SELECT INDEX_NAME, TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.INDEXES");
            return
                items.Select(
                    item => new Tuple<string, string, string>(item.TABLE_NAME, item.COLUMN_NAME, item.CONSTRAINT_NAME))
                     .Union(
                         indexItems.Select(
                             indexItem =>
                             new Tuple<string, string, string>(indexItem.TABLE_NAME, indexItem.COLUMN_NAME,
                                                               indexItem.INDEX_NAME))).ToList();
        }

        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(Database db)
        {
            var items =
                db.Fetch<dynamic>(
                    @"SELECT TABLE_NAME, INDEX_NAME, COLUMN_NAME, [UNIQUE] FROM INFORMATION_SCHEMA.INDEXES 
WHERE INDEX_NAME NOT LIKE 'PK_%'
ORDER BY TABLE_NAME, INDEX_NAME");
            return
                items.Select(
                    item => new Tuple<string, string, string, bool>(item.TABLE_NAME, item.INDEX_NAME, item.COLUMN_NAME, item.UNIQUE));
        }

        public override bool DoesTableExist(Database db, string tableName)
        {
            var result =
                db.ExecuteScalar<long>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName",
                                       new { TableName = tableName });

            return result > 0;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            if (column.Seeding != default(int))
                return string.Format("IDENTITY({0},1)", column.Seeding);

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
            get
            {
                return "ALTER TABLE [{0}] ALTER COLUMN [{1}] DROP DEFAULT";
            }
        }

        

        public override string DropIndex { get { return "DROP INDEX {1}.{0}"; } }
        
    }
}