using Umbraco.Core.Persistence.Migrations.Model;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateConstraintExpression : IMigrationExpression
    {
        public CreateConstraintExpression(ConstraintType type)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; private set; }

        public override string ToString()
        {
            //TODO replace with sql syntax provider
            return base.ToString() + Constraint.ConstraintName;
        }
    }
}