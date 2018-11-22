using Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Table
{
    public class DeleteTableBuilder : ExpressionBuilderBase<DeleteTableExpression>
    {
        public DeleteTableBuilder(DeleteTableExpression expression) : base(expression)
        {
        }
    }
}