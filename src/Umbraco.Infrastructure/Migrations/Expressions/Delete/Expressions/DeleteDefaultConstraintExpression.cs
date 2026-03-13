namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents an expression used in a database migration to delete a default constraint from a table column.
/// </summary>
public class DeleteDefaultConstraintExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDefaultConstraintExpression"/> class,
    /// which is used to define an expression for deleting a default constraint in a database migration.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> providing migration-specific information and services.</param>
    public DeleteDefaultConstraintExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table from which the default constraint will be deleted.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the name of the column associated with the default constraint to delete.
    /// </summary>
    public virtual string? ColumnName { get; set; }

    /// <summary>
    /// Gets or sets the name of the default constraint to delete.
    /// </summary>
    public virtual string? ConstraintName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a default constraint is present on the column.
    /// </summary>
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
