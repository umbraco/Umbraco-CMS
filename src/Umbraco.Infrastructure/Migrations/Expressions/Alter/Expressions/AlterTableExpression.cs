namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;

public class AlterTableExpression : MigrationExpressionBase
{
    public AlterTableExpression(IMigrationContext context)
        : base(context)
    {
    }

    public virtual string? TableName { get; set; }

    protected override string GetSql() => string.Empty;
}
