using Umbraco.Core.Migrations.Expressions.Delete.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Table
{
    /// <summary>
    /// Builds a Delete Table expression, and executes.
    /// </summary>
    public class DeleteTableBuilder : ExpressionBuilderBase<DeleteTableExpression>
    {
        public DeleteTableBuilder(DeleteTableExpression expression)
            : base(expression)
        {
            Expression.Execute();
        }
    }
}
