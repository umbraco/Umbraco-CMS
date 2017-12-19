using NPoco;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Migrations.Expressions.Rename.Expressions
{
    public class RenameColumnExpression : MigrationExpressionBase
    {
        public RenameColumnExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string TableName { get; set; }
        public virtual string OldName { get; set; }
        public virtual string NewName { get; set; }

        public override string Process(IMigrationContext context)
            => GetSql();

        public override string ToString() // fixme kill
                => GetBaseSql();

        /// <inheritdoc />
        protected override string GetSql()
        {
            return DatabaseType.IsMySql()
                ? GetMySql()
                : GetBaseSql();
        }

        private string GetBaseSql()
        {
            return IsExpressionSupported() == false
                ? string.Empty
                : SqlSyntax.FormatColumnRename(TableName, OldName, NewName);
        }

        private string GetMySql()
        {
            var columnDefinitionSql = $@"
SELECT CONCAT(
    CAST(COLUMN_TYPE AS CHAR),
    IF(ISNULL(CHARACTER_SET_NAME), '', CONCAT(' CHARACTER SET ', CHARACTER_SET_NAME)),
    IF(ISNULL(COLLATION_NAME), '', CONCAT(' COLLATE ', COLLATION_NAME)),
    ' ',
    IF(IS_NULLABLE = 'NO', 'NOT NULL ', ''),
    IF(IS_NULLABLE = 'NO' AND COLUMN_DEFAULT IS NULL, '', CONCAT('DEFAULT ', QUOTE(COLUMN_DEFAULT), ' ')),
    UPPER(extra))
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '{TableName}' AND COLUMN_NAME = '{OldName}'";

            var columnDefinition = Database.ExecuteScalar<string>(columnDefinitionSql);
            return GetBaseSql() + " " + columnDefinition;
        }
    }
}
