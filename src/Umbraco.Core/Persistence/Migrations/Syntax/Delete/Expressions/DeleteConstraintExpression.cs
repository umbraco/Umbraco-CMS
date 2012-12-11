using Umbraco.Core.Persistence.DatabaseModelDefinitions;

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
            //TODO change to use sql syntax provider
            return base.ToString() + Constraint.ConstraintName;
        }
    }
}