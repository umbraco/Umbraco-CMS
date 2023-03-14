using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common.Expressions;

public class CreateIndexExpression : MigrationExpressionBase
{
    public CreateIndexExpression(IMigrationContext context, IndexDefinition index)
        : base(context) =>
        Index = index;

    public CreateIndexExpression(IMigrationContext context)
        : base(context) =>
        Index = new IndexDefinition();

    public IndexDefinition Index { get; set; }

    protected override string GetSql() => SqlSyntax.Format(Index);
}
