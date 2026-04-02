using System.Text;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

/// <summary>
/// Represents a database migration expression that deletes data from a specified table.
/// Used within migration operations to define data deletion actions.
/// </summary>
public class DeleteDataExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteDataExpression"/> class using the specified migration context.
    /// This expression is used to define data deletion operations within a migration.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> providing migration-specific information and services.</param>
    public DeleteDataExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table from which data is to be deleted.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the delete operation targets all rows.
    /// </summary>
    public virtual bool IsAllRows { get; set; }

    /// <summary>
    /// Gets the collection of data row definitions that specify which rows should be deleted.
    /// </summary>
    public List<DeletionDataDefinition> Rows { get; } = new();

    protected override string GetSql()
    {
        if (IsAllRows)
        {
            return string.Format(SqlSyntax.DeleteData, SqlSyntax.GetQuotedTableName(TableName), "(1=1)");
        }

        var stmts = new StringBuilder();
        foreach (DeletionDataDefinition row in Rows)
        {
            IEnumerable<string> whereClauses = row.Select(kvp =>
                $"{SqlSyntax.GetQuotedColumnName(kvp.Key)} {(kvp.Value == null ? "IS" : "=")} {GetQuotedValue(kvp.Value)}");

            stmts.Append(string.Format(
                SqlSyntax.DeleteData,
                SqlSyntax.GetQuotedTableName(TableName),
                string.Join(" AND ", whereClauses)));

            AppendStatementSeparator(stmts);
        }

        return stmts.ToString();
    }
}
