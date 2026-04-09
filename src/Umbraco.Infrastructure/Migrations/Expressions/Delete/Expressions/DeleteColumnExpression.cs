using System.Text;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents an expression to delete a column from a database table.
/// </summary>
public class DeleteColumnExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions.DeleteColumnExpression"/> class,
    /// which is used to define a column deletion operation within a database migration.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> providing migration-specific information and services.</param>
    public DeleteColumnExpression(IMigrationContext context)
        : base(context) =>
        ColumnNames = new List<string>();

    /// <summary>
    /// Gets or sets the name of the table from which the column will be deleted.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the collection of column names that are to be deleted from the table.
    /// </summary>
    public ICollection<string> ColumnNames { get; set; }

    protected override string GetSql()
    {
        var stmts = new StringBuilder();
        foreach (var columnName in ColumnNames)
        {
            stmts.AppendFormat(
                SqlSyntax.DropColumn,
                SqlSyntax.GetQuotedTableName(TableName),
                SqlSyntax.GetQuotedColumnName(columnName));
            AppendStatementSeparator(stmts);
        }

        return stmts.ToString();
    }
}
