using Umbraco.Core.Migrations.Expressions.Rename.Expressions;

namespace Umbraco.Core.Migrations.Expressions.Rename.Table
{
    /// <summary>
    /// Implements <see cref="IRenameTableBuilder"/>.
    /// </summary>
    public class RenameTableBuilder : ExpressionBuilderBase<RenameTableExpression>, IRenameTableBuilder
    {
        public RenameTableBuilder(RenameTableExpression expression)
            : base(expression)
        { }

        /// <inheritdoc />
        public void To(string name)
        {
            Expression.NewName = name;
            Expression.Execute();
        }
    }
}
