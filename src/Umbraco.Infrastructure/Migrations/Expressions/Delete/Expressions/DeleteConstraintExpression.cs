using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

public class DeleteConstraintExpression : MigrationExpressionBase
{
    public DeleteConstraintExpression(IMigrationContext context, ConstraintType type)
        : base(context) =>
        Constraint = new ConstraintDefinition(type);

    public ConstraintDefinition Constraint { get; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.DeleteConstraint,
            SqlSyntax.GetQuotedTableName(Constraint.TableName),
            SqlSyntax.GetQuotedName(Constraint.ConstraintName));
}
