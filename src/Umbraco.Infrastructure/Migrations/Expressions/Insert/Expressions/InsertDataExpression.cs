using System.Text;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.Expressions;

/// <summary>
/// Represents an expression to insert data into a database table during a migration.
/// </summary>
public class InsertDataExpression : MigrationExpressionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertDataExpression"/> class with the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the insert data expression.</param>
    public InsertDataExpression(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// Gets or sets the name of the table to insert data into.
    /// </summary>
    public string? TableName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether identity insert is enabled during data insertion.
    /// </summary>
    public bool EnabledIdentityInsert { get; set; }

    /// <summary>
    /// Gets the list of <see cref="InsertionDataDefinition"/> objects representing the rows to be inserted.
    /// </summary>
    public List<InsertionDataDefinition> Rows { get; } = new();

    protected override string GetSql()
    {
        var stmts = new StringBuilder();

        if (EnabledIdentityInsert && SqlSyntax.SupportsIdentityInsert())
        {
            stmts.AppendLine($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(TableName)} ON");
            AppendStatementSeparator(stmts);
        }

        try
        {
            foreach (InsertionDataDefinition item in Rows)
            {
                var cols = new StringBuilder();
                var vals = new StringBuilder();
                var first = true;
                foreach (KeyValuePair<string, object?> keyVal in item)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        cols.Append(",");
                        vals.Append(",");
                    }

                    cols.Append(SqlSyntax.GetQuotedColumnName(keyVal.Key));
                    vals.Append(GetQuotedValue(keyVal.Value));
                }

                var sql = string.Format(SqlSyntax.InsertData, SqlSyntax.GetQuotedTableName(TableName), cols, vals);

                stmts.Append(sql);
                AppendStatementSeparator(stmts);
            }
        }
        finally
        {
            if (EnabledIdentityInsert && SqlSyntax.SupportsIdentityInsert())
            {
                stmts.AppendLine($"SET IDENTITY_INSERT {SqlSyntax.GetQuotedTableName(TableName)} OFF");
                AppendStatementSeparator(stmts);
            }
        }

        return stmts.ToString();
    }
}
