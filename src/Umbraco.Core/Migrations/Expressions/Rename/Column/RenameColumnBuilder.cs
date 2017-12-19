using Umbraco.Core.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Rename.Column
{
    /// <summary>
    /// Implements <see cref="IRenameColumnBuilder"/>, <see cref="IRenameColumnToBuilder"/>.
    /// </summary>
    public class RenameColumnBuilder : ExpressionBuilderBase<RenameColumnExpression>, IRenameColumnToBuilder, IRenameColumnBuilder
    {
        public RenameColumnBuilder(RenameColumnExpression expression) : base(expression)
        { }

        /// <inheritdoc />
        public void To(string name)
        {
            Expression.NewName = name;
            Expression.Execute();
        }

        /// <inheritdoc />
        public IRenameColumnToBuilder OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }
    }
}
