using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;

/// <summary>
/// Represents a migration expression used to create a database index.
/// </summary>
public class CreateIndexExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions.CreateIndexExpression"/> class with the specified migration context and index definition.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration operation.</param>
    /// <param name="index">The <see cref="IndexDefinition"/> that defines the index to be created.</param>
    public CreateIndexExpression(IMigrationContext context, IndexDefinition index)
        : base(context) =>
        Index = index;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateIndexExpression"/> class using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the index creation expression.</param>
    public CreateIndexExpression(IMigrationContext context)
        : base(context) =>
        Index = new IndexDefinition();

    /// <summary>
    /// Gets or sets the definition of the index to be created.
    /// </summary>
    public IndexDefinition Index { get; set; }

    protected override string GetSql() => SqlSyntax.Format(Index);
}
