namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents a database migration expression that deletes a table.
/// This is used to define the removal of a table during a migration process.
/// </summary>
public class DeleteTableExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTableExpression"/> class.
    /// </summary>
    /// <param name="context">An <see cref="IMigrationContext"/> that provides information and services for the migration operation.</param>
    public DeleteTableExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table to be deleted.
    /// </summary>
    public virtual string? TableName { get; set; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.DropTable,
            SqlSyntax.GetQuotedTableName(TableName));
}
