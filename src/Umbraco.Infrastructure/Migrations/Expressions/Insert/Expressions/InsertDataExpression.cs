using System.Text;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.Expressions;

public class InsertDataExpression : MigrationExpressionBase
{
    public InsertDataExpression(IMigrationContext context)
        : base(context)
    {
    }

    public string? TableName { get; set; }

    public bool EnabledIdentityInsert { get; set; }

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
