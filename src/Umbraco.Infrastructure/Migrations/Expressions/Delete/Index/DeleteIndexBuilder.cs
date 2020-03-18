using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Delete.Expressions;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Index
{
    public class DeleteIndexBuilder : ExpressionBuilderBase<DeleteIndexExpression>,
        IDeleteIndexForTableBuilder, IExecutableBuilder
    {
        public DeleteIndexBuilder(DeleteIndexExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void Do() => Expression.Execute();

        public IExecutableBuilder OnTable(string tableName)
        {
            Expression.Index.TableName = tableName;
            return this;
        }
    }
}
