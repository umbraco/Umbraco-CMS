using NPoco;

namespace Umbraco.Core.Migrations.Expressions.Delete.Expressions
{
    public class DeleteTableExpression : MigrationExpressionBase
    {
        public DeleteTableExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        { }

        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return string.Format(SqlSyntax.DropTable,
                SqlSyntax.GetQuotedTableName(TableName));
        }
    }
}
