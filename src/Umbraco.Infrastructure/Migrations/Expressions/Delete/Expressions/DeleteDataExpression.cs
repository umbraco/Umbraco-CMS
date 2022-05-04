using System.Text;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

public class DeleteDataExpression : MigrationExpressionBase
{
    public DeleteDataExpression(IMigrationContext context)
        : base(context)
    {
    }

    public string? TableName { get; set; }

    public virtual bool IsAllRows { get; set; }

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
