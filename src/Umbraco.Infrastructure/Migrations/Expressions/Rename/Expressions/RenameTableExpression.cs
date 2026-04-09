namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

/// <summary>
///     Represents a Rename Table expression.
/// </summary>
public class RenameTableExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions.RenameTableExpression"/> class,
    /// which is used to define a table rename operation within a migration.
    /// </summary>
    /// <param name="context">The migration context that provides information and services for the migration process.</param>
    public RenameTableExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    ///     Gets or sets the source name.
    /// </summary>
    public virtual string? OldName { get; set; }

    /// <summary>
    ///     Gets or sets the target name.
    /// </summary>
    public virtual string? NewName { get; set; }

    /// <inheritdoc />
    /// <inheritdoc />
    protected override string GetSql() => SqlSyntax.FormatTableRename(OldName, NewName);
}
