using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteConstraintExpression : IMigrationExpression
    {
        public DeleteConstraintExpression(ConstraintType type)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; private set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.DeleteConstraint,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(Constraint.TableName),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedName(Constraint.ConstraintName));
        }
    }
}