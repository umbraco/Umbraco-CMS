namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

public class DeleteDefaultConstraintExpression : MigrationExpressionBase
{
    public DeleteDefaultConstraintExpression(IMigrationContext context)
        : base(context)
    {
    }

    public virtual string? TableName { get; set; }

    public virtual string? ColumnName { get; set; }

    public virtual string? ConstraintName { get; set; }

    public virtual bool HasDefaultConstraint { get; set; }

    protected override string GetSql() =>
        HasDefaultConstraint
            ? string.Format(
                SqlSyntax.DeleteDefaultConstraint,
                SqlSyntax.GetQuotedTableName(TableName),
                SqlSyntax.GetQuotedColumnName(ColumnName),
                SqlSyntax.GetQuotedName(ConstraintName))
            : string.Empty;
}
