using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteConstraintExpression : MigrationExpressionBase
    {
        public DeleteConstraintExpression(ConstraintType type)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; private set; }

        public override string ToString()
        {
            return string.Format(SqlSyntaxContext.SqlSyntaxProvider.DeleteConstraint,
                                 SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(Constraint.TableName),
                                 SqlSyntaxContext.SqlSyntaxProvider.GetQuotedName(Constraint.ConstraintName));
        }
    }
}