using Umbraco.Core.Migrations.Syntax.Rename.Expressions;

namespace Umbraco.Core.Migrations.Syntax.Rename.Table
{
    public class RenameTableBuilder : ExpressionBuilderBase<RenameTableExpression>, IRenameTableSyntax
    {
        public RenameTableBuilder(RenameTableExpression expression) : base(expression)
        {
        }

        public void To(string name)
        {
            Expression.NewName = name;
        }
    }
}
