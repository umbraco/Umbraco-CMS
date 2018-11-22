namespace Umbraco.Core.Persistence.Migrations.Syntax
{
    public abstract class ExpressionBuilderBase<T>
        where T : IMigrationExpression
    {
        public T Expression { get; private set; }

        protected ExpressionBuilderBase(T expression)
        {
            Expression = expression;
        }
    }
}