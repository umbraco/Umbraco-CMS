using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Rename.Column
{
    public class RenameColumnBuilder : ExpressionBuilderBase<RenameColumnExpression>,
        IRenameColumnToBuilder, IRenameColumnBuilder, IExecutableBuilder
    {
        public RenameColumnBuilder(RenameColumnExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void Do() => Expression.Execute();

        /// <inheritdoc />
        public IExecutableBuilder To(string name)
        {
            Expression.NewName = name;
            return this;
        }

        /// <inheritdoc />
        public IRenameColumnToBuilder OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }
    }
}
