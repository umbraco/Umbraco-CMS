using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;

/// <summary>
/// Represents an expression used to define a foreign key constraint as part of a database migration.
/// This expression specifies the relationship between tables by linking columns in the source and target tables.
/// </summary>
public class CreateForeignKeyExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateForeignKeyExpression"/> class with the specified migration context and foreign key definition.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration.</param>
    /// <param name="fkDef">The <see cref="ForeignKeyDefinition"/> that defines the foreign key.</param>
    public CreateForeignKeyExpression(IMigrationContext context, ForeignKeyDefinition fkDef)
        : base(context) =>
        ForeignKey = fkDef;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateForeignKeyExpression"/> class using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to be used for the migration operation.</param>
    public CreateForeignKeyExpression(IMigrationContext context)
        : base(context) =>
        ForeignKey = new ForeignKeyDefinition();

    /// <summary>
    /// Gets or sets the <see cref="ForeignKeyDefinition"/> representing the foreign key associated with this expression.
    /// </summary>
    public ForeignKeyDefinition ForeignKey { get; set; }

    protected override string GetSql() => SqlSyntax.Format(ForeignKey);
}
