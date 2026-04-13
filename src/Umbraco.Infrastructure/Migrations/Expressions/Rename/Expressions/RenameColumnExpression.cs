namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

/// <summary>
/// Represents an expression to rename a column in a database table.
/// </summary>
public class RenameColumnExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RenameColumnExpression"/> class using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the rename column operation.</param>
    public RenameColumnExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table containing the column to rename.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the previous name of the column to be renamed.
    /// </summary>
    public virtual string? OldName { get; set; }

    /// <summary>
    /// Gets or sets the new name to assign to the column during the rename operation.
    /// </summary>
    public virtual string? NewName { get; set; }

    /// <inheritdoc />
    protected override string GetSql() => SqlSyntax.FormatColumnRename(TableName, OldName, NewName);
}
