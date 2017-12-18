using Umbraco.Core.Migrations.Syntax.Rename.Expressions;

namespace Umbraco.Core.Migrations.Syntax.Rename.Column
{
    public class RenameColumnBuilder : ExpressionBuilderBase<RenameColumnExpression>, IRenameColumnToSyntax, IRenameColumnTableSyntax
    {
        public RenameColumnBuilder(RenameColumnExpression expression) : base(expression)
        {
        }

        public void To(string name)
        {
            Expression.NewName = name;
        }

        public IRenameColumnToSyntax OnTable(string tableName)
        {
            Expression.TableName = tableName;
            return this;
        }
    }
}
