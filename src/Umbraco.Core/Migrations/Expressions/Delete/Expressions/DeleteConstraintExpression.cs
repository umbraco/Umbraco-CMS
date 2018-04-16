using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Delete.Expressions
{
    public class DeleteConstraintExpression : MigrationExpressionBase
    {
        public DeleteConstraintExpression(IMigrationContext context, ConstraintType type)
            : base(context)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; }

        protected override string GetSql()
        {
            return DatabaseType.IsMySql()
                ? GetMySql()
                : string.Format(SqlSyntax.DeleteConstraint,
                    SqlSyntax.GetQuotedTableName(Constraint.TableName),
                    SqlSyntax.GetQuotedName(Constraint.ConstraintName));
        }

        private string GetMySql()
        {
            return string.Format(SqlSyntax.DeleteConstraint,
                SqlSyntax.GetQuotedTableName(Constraint.TableName),
                Constraint.IsPrimaryKeyConstraint ? "PRIMARY KEY" : "FOREIGN KEY",
                "");
        }
    }
}
