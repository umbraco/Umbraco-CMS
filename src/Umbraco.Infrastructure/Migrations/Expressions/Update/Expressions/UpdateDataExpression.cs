namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update.Expressions;

/// <summary>
/// Represents an expression used to update records in a database table during a migration.
/// </summary>
public class UpdateDataExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateDataExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the update data expression.</param>
    public UpdateDataExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the database table to be updated by this expression.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets the collection of column-value pairs specifying the data to update.
    /// </summary>
    public List<KeyValuePair<string, object?>>? Set { get; set; }

    /// <summary>
    /// Gets or sets the collection of conditions used to identify which records should be updated in the data store.
    /// Each condition is represented as a key-value pair, where the key is the column name and the value is the value to match.
    /// </summary>
    public List<KeyValuePair<string, object?>>? Where { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the update applies to all rows.
    /// </summary>
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
