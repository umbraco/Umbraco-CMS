namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update.Expressions;

public class UpdateDataExpression : MigrationExpressionBase
{
    public UpdateDataExpression(IMigrationContext context)
        : base(context)
    {
    }

    public string? TableName { get; set; }

    public List<KeyValuePair<string, object?>>? Set { get; set; }

    public List<KeyValuePair<string, object?>>? Where { get; set; }

    public bool IsAllRows { get; set; }

    protected override string GetSql()
    {
        IEnumerable<string>? updateItems =
            Set?.Select(x => $"{SqlSyntax.GetQuotedColumnName(x.Key)} = {GetQuotedValue(x.Value)}");
        IEnumerable<string>? whereClauses = IsAllRows
            ? null
            : Where?.Select(x =>
                $"{SqlSyntax.GetQuotedColumnName(x.Key)} {(x.Value == null ? "IS" : "=")} {GetQuotedValue(x.Value)}");

        var whereClause = whereClauses == null
            ? "(1=1)"
            : string.Join(" AND ", whereClauses.ToArray());

        return string.Format(
            SqlSyntax.UpdateData,
            SqlSyntax.GetQuotedTableName(TableName),
            string.Join(", ", updateItems ?? Array.Empty<string>()),
            whereClause);
    }
}
