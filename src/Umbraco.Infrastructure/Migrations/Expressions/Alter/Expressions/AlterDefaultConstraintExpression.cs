namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;

public class AlterDefaultConstraintExpression : MigrationExpressionBase
{
    public AlterDefaultConstraintExpression(IMigrationContext context)
        : base(context)
    {
    }

    public virtual string? TableName { get; set; }

    public virtual string? ColumnName { get; set; }

    public virtual string? ConstraintName { get; set; }

    public virtual object? DefaultValue { get; set; }

    protected override string GetSql() =>

        // NOTE Should probably investigate if Deleting a Default Constraint is different from deleting a 'regular' constraint
        string.Format(
            SqlSyntax.DeleteConstraint,
            SqlSyntax.GetQuotedTableName(TableName),
            SqlSyntax.GetQuotedName(ConstraintName));
}
