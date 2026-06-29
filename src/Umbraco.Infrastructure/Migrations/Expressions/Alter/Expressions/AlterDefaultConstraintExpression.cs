namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;

/// <summary>
/// Represents an expression to alter a default constraint in a database migration.
/// </summary>
public class AlterDefaultConstraintExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions.AlterDefaultConstraintExpression"/> class using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the alter default constraint expression.</param>
    public AlterDefaultConstraintExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table to which the default constraint applies.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the column to which the default constraint applies.
    /// </summary>
    public virtual string? ColumnName { get; set; }

    /// <summary>Gets or sets the name of the default constraint to be altered.</summary>
    public virtual string? ConstraintName { get; set; }

    /// <summary>
    /// Gets or sets the value to be used as the default for the constraint.
    /// </summary>
    public virtual object? DefaultValue { get; set; }

    protected override string GetSql() =>

        // NOTE Should probably investigate if Deleting a Default Constraint is different from deleting a 'regular' constraint
        string.Format(
            SqlSyntax.DeleteConstraint,
            SqlSyntax.GetQuotedTableName(TableName),
            SqlSyntax.GetQuotedName(ConstraintName));
}
