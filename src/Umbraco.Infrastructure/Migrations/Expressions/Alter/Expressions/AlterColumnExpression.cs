using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;

public class AlterColumnExpression : MigrationExpressionBase
{
    public AlterColumnExpression(IMigrationContext context)
        : base(context) =>
        Column = new ColumnDefinition { ModificationType = ModificationType.Alter };

    public virtual string? SchemaName { get; set; }

    public virtual string? TableName { get; set; }

    public virtual ColumnDefinition Column { get; set; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.AlterColumn,
            SqlSyntax.GetQuotedTableName(TableName),
            SqlSyntax.Format(Column));
}
