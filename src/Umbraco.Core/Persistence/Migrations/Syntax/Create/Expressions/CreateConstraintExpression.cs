using System.Linq;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Create.Expressions
{
    public class CreateConstraintExpression : MigrationExpressionBase
    {
        public CreateConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, ConstraintType constraint) 
            : base(current, databaseProviders, sqlSyntax)
        {
            Constraint = new ConstraintDefinition(constraint);
        }
        

        public ConstraintDefinition Constraint { get; private set; }

        public override string ToString()
        {
            var constraintType = (Constraint.IsPrimaryKeyConstraint) ? "PRIMARY KEY" : "UNIQUE";

            if (Constraint.IsPrimaryKeyConstraint && SqlSyntax.SupportsClustered())
                constraintType += " CLUSTERED";

            if (Constraint.IsNonUniqueConstraint)
                constraintType = string.Empty;

            var columns = new string[Constraint.Columns.Count];

            for (int i = 0; i < Constraint.Columns.Count; i++)
            {
                columns[i] = SqlSyntax.GetQuotedColumnName(Constraint.Columns.ElementAt(i));
            }

            return string.Format(SqlSyntax.CreateConstraint,
                                 SqlSyntax.GetQuotedTableName(Constraint.TableName),
                                 SqlSyntax.GetQuotedName(Constraint.ConstraintName),
                                 constraintType,
                                 string.Join(", ", columns));
        }
    }
}