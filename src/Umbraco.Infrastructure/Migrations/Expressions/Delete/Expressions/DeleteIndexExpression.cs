using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Expressions;

public class DeleteIndexExpression : MigrationExpressionBase
{
    public DeleteIndexExpression(IMigrationContext context)
        : base(context) =>
        Index = new IndexDefinition();

    public DeleteIndexExpression(IMigrationContext context, IndexDefinition index)
        : base(context) =>
        Index = index;

    public IndexDefinition Index { get; }

    protected override string GetSql() =>
        string.Format(
            SqlSyntax.DropIndex,
            SqlSyntax.GetQuotedName(Index.Name),
            SqlSyntax.GetQuotedTableName(Index.TableName));
}
