using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions
{
    public class RenameColumnExpression : MigrationExpressionBase
    {        
        public RenameColumnExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax)
            : base(current, databaseProviders, sqlSyntax)
        {
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }

        public override string Process(Database database)
        {
            if (CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                string columnDefinitionSql = string.Format(@"
SELECT CONCAT(
          CAST(COLUMN_TYPE AS CHAR),
          IF(ISNULL(CHARACTER_SET_NAME),
             '',
             CONCAT(' CHARACTER SET ', CHARACTER_SET_NAME)),
          IF(ISNULL(COLLATION_NAME),
             '',
             CONCAT(' COLLATE ', COLLATION_NAME)),
          ' ',
          IF(IS_NULLABLE = 'NO', 'NOT NULL ', ''),
          IF(IS_NULLABLE = 'NO' AND COLUMN_DEFAULT IS NULL,
             '',
             CONCAT('DEFAULT ', QUOTE(COLUMN_DEFAULT), ' ')),
          UPPER(extra))
  FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", TableName, OldName);

                var columnDefinition = database.ExecuteScalar<string>(columnDefinitionSql);
                return this.ToString() + " " + columnDefinition;
            }

            return this.ToString();
        }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlSyntax.FormatColumnRename(TableName, OldName, NewName);
        }
    }
}