namespace Umbraco.Core.Migrations.Expressions.Common
{
    public class ExecutableBuilder : IExecutableBuilder
    {
        private readonly IMigrationExpression _expression;

        public ExecutableBuilder(IMigrationExpression expression)
        {
            _expression = expression;
        }

        /// <inheritdoc />
        public void Do() => _expression.Execute();
    }
}
