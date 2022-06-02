namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;

public class RenameColumnExpression : MigrationExpressionBase
{
    public RenameColumnExpression(IMigrationContext context)
        : base(context)
    {
    }

    public virtual string? TableName { get; set; }

    public virtual string? OldName { get; set; }

    public virtual string? NewName { get; set; }

    /// <inheritdoc />
    protected override string GetSql() => SqlSyntax.FormatColumnRename(TableName, OldName, NewName);
}
