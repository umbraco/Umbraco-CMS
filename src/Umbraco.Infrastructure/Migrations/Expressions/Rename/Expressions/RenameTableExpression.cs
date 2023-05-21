namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

/// <summary>
///     Represents a Rename Table expression.
/// </summary>
public class RenameTableExpression : MigrationExpressionBase
{
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
