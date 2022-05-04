using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;

public class CreateConstraintExpression : MigrationExpressionBase
{
    public CreateConstraintExpression(IMigrationContext context, ConstraintType constraint)
        : base(context) =>
        Constraint = new ConstraintDefinition(constraint);

    public ConstraintDefinition Constraint { get; }

    protected override string GetSql()
    {
        var constraintType = Constraint.IsPrimaryKeyConstraint ? "PRIMARY KEY" : "UNIQUE";

        if (Constraint.IsPrimaryKeyConstraint && SqlSyntax.SupportsClustered())
        {
            constraintType += Constraint.IsPrimaryKeyClustered ? " CLUSTERED" : " NONCLUSTERED";
        }

        if (Constraint.IsNonUniqueConstraint)
        {
            constraintType = string.Empty;
        }

        var columns = new string[Constraint.Columns.Count];

        for (var i = 0; i < Constraint.Columns.Count; i++)
        {
            columns[i] = SqlSyntax.GetQuotedColumnName(Constraint.Columns.ElementAt(i));
        }

        return string.Format(
            SqlSyntax.CreateConstraint,
            SqlSyntax.GetQuotedTableName(Constraint.TableName),
            SqlSyntax.GetQuotedName(Constraint.ConstraintName),
            constraintType,
            string.Join(", ", columns));
    }
}
