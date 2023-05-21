namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

public class DeleteTableExpression : MigrationExpressionBase
{
    public DeleteTableExpression(IMigrationContext context)
        : base(context)
    {
    }

    public virtual string? TableName { get; set; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.DropTable,
            SqlSyntax.GetQuotedTableName(TableName));
}
