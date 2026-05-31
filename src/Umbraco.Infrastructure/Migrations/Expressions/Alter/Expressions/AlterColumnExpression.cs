using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;

/// <summary>
/// Represents a migration expression used to alter the definition of an existing column in a database table.
/// This may include changing the column's type, constraints, or other properties.
/// </summary>
public class AlterColumnExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlterColumnExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the migration operation.</param>
    public AlterColumnExpression(IMigrationContext context)
        : base(context) =>
        Column = new ColumnDefinition { ModificationType = ModificationType.Alter };

    /// <summary>
    /// Gets or sets the name of the schema that contains the column to be altered.
    /// </summary>
    public virtual string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the name of the table containing the column to be altered.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the definition of the column to be altered.
    /// </summary>
    public virtual ColumnDefinition Column { get; set; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.AlterColumn,
            SqlSyntax.GetQuotedTableName(TableName),
            SqlSyntax.Format(Column));
}
