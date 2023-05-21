using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;

public class CreateColumnExpression : MigrationExpressionBase
{
    public CreateColumnExpression(IMigrationContext context)
        : base(context) =>
        Column = new ColumnDefinition { ModificationType = ModificationType.Create };

    public string? TableName { get; set; }

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
