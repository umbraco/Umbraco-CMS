using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;

/// <summary>
/// Represents an expression to create a new column in a database table.
/// </summary>
public class CreateColumnExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions.CreateColumnExpression"/> class,
    /// using the specified migration context to provide migration-related information and services.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> that supplies context and services for the migration operation.</param>
    public CreateColumnExpression(IMigrationContext context)
        : base(context) =>
        Column = new ColumnDefinition { ModificationType = ModificationType.Create };

    /// <summary>
    /// Gets or sets the name of the table to which the column will be added.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ColumnDefinition"/> representing the column to be created in the migration.
    /// </summary>
    public ColumnDefinition Column { get; set; }

    protected override string GetSql()
    {
        if (string.IsNullOrEmpty(Column.TableName))
        {
            Column.TableName = TableName;
        }

        return string.Format(
            SqlSyntax.AddColumn,
            SqlSyntax.GetQuotedTableName(Column.TableName),
            SqlSyntax.Format(Column));
    }
}
