using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;

/// <summary>
/// Represents a migration expression that executes a specified raw SQL statement.
/// This is typically used to perform custom database operations during a migration.
/// </summary>
public class ExecuteSqlStatementExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecuteSqlStatementExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration operation.</param>
    public ExecuteSqlStatementExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the SQL statement to be executed.
    /// </summary>
    public virtual string? SqlStatement { get; set; }

    /// <summary>
    /// Gets or sets the SQL object that represents the SQL statement to be executed.
    /// </summary>
    public virtual Sql<ISqlContext>? SqlObject { get; set; }

    /// <summary>
    /// Executes the SQL statement encapsulated in the <see cref="SqlObject"/> property.
    /// </summary>
    public void ExecuteSqlObject() => Execute(SqlObject);

    protected override string? GetSql() => SqlStatement;
}
