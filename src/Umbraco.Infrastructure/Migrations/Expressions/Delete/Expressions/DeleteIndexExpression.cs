using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents a migration expression that specifies the deletion of a database index.
/// Used during schema migrations to remove an existing index from a database table.
/// </summary>
public class DeleteIndexExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteIndexExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the delete index operation.</param>
    public DeleteIndexExpression(IMigrationContext context)
        : base(context) =>
        Index = new IndexDefinition();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteIndexExpression"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration operation.</param>
    /// <param name="index">The <see cref="IndexDefinition"/> representing the index to be deleted.</param>
    public DeleteIndexExpression(IMigrationContext context, IndexDefinition index)
        : base(context) =>
        Index = index;

    /// <summary>
    /// Gets the index definition to be deleted.
    /// </summary>
    public IndexDefinition Index { get; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.DropIndex,
            SqlSyntax.GetQuotedName(Index.Name),
            SqlSyntax.GetQuotedTableName(Index.TableName));
}
