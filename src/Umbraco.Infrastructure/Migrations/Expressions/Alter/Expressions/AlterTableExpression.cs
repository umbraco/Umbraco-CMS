namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;

/// <summary>
/// Represents an expression to alter an existing table in the database schema.
/// </summary>
public class AlterTableExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlterTableExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration.</param>
    public AlterTableExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table to be altered.
    /// </summary>
    public virtual string? TableName { get; set; }

    protected override string GetSql() => string.Empty;
}
