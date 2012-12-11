using Umbraco.Core.Persistence.Migrations.Syntax.Rename.Expressions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Rename.Column
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