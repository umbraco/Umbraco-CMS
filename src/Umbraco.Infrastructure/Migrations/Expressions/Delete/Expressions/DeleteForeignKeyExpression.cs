using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents a database migration expression that deletes a foreign key constraint from a table.
/// </summary>
public class DeleteForeignKeyExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteForeignKeyExpression"/> class, used to define a database migration expression for deleting a foreign key.
    /// </summary>
    /// <param name="context">The migration context that provides information and services for the migration process.</param>
    public DeleteForeignKeyExpression(IMigrationContext context)
        : base(context) =>
        ForeignKey = new ForeignKeyDefinition();

    /// <summary>
    /// Gets or sets the definition of the foreign key that will be deleted by this expression.
    /// </summary>
    public ForeignKeyDefinition ForeignKey { get; set; }

    protected override string GetSql()
    {
        if (ForeignKey.ForeignTable == null)
        {
            throw new ArgumentNullException(
                "Table name not specified, ensure you have appended the OnTable extension. Format should be Delete.ForeignKey(KeyName).OnTable(TableName)");
        }

        if (string.IsNullOrEmpty(ForeignKey.Name))
        {
            ForeignKey.Name =
                $"FK_{ForeignKey.ForeignTable}_{ForeignKey.PrimaryTable}_{ForeignKey.PrimaryColumns.First()}";
        }

        return string.Format(
            SqlSyntax.DeleteConstraint,
            SqlSyntax.GetQuotedTableName(ForeignKey.ForeignTable),
            SqlSyntax.GetQuotedName(ForeignKey.Name));
    }
}
