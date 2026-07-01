using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents an expression to delete a database constraint during a migration.
/// </summary>
public class DeleteConstraintExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteConstraintExpression"/> class with the specified migration context and constraint type.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration operation.</param>
    /// <param name="type">The <see cref="ConstraintType"/> representing the type of constraint to delete.</param>
    public DeleteConstraintExpression(IMigrationContext context, ConstraintType type)
        : base(context) =>
        Constraint = new ConstraintDefinition(type);

    /// <summary>
    /// Gets the constraint definition to be deleted.
    /// </summary>
    public ConstraintDefinition Constraint { get; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.DeleteConstraint,
            SqlSyntax.GetQuotedTableName(Constraint.TableName),
            SqlSyntax.GetQuotedName(Constraint.ConstraintName));
}
