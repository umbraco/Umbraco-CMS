using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;

/// <summary>
/// Represents an expression used to define and create a database constraint during a migration.
/// </summary>
public class CreateConstraintExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateConstraintExpression"/> class with the specified migration context and constraint type.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration.</param>
    /// <param name="constraint">The <see cref="ConstraintType"/> representing the type of constraint to create.</param>
    public CreateConstraintExpression(IMigrationContext context, ConstraintType constraint)
        : base(context) =>
        Constraint = new ConstraintDefinition(constraint);

    /// <summary>
    /// Gets the constraint definition.
    /// </summary>
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
