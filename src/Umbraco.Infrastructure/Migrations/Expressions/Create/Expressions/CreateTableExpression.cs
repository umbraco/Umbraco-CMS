using System.Collections.Generic;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions;

/// <summary>
/// Represents a migration expression used to create a new database table.
/// </summary>
public class CreateTableExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Expressions.CreateTableExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the table creation expression.</param>
    public CreateTableExpression(IMigrationContext context)
        : base(context) =>
        Columns = new List<ColumnDefinition>();

    /// <summary>
    /// Gets or sets the database schema name in which the table will be created.
    /// </summary>
    public virtual string SchemaName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the name of the table to be created.
    /// </summary>
    public virtual string TableName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the collection of column definitions for the table.
    /// </summary>
    public virtual IList<ColumnDefinition> Columns { get; set; }

    protected override string GetSql()
    {
        var foreignKeys = Expressions
                .OfType<CreateForeignKeyExpression>()
                .Select(x => x.ForeignKey)
                .ToList();

            var table = new TableDefinition { Name = TableName, SchemaName = SchemaName, Columns = Columns, ForeignKeys = foreignKeys };

        return string.Format(SqlSyntax.Format(table));
    }
}
