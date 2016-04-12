using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteConstraintExpression : MigrationExpressionBase
    {
        public DeleteConstraintExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes, ConstraintType type)
            : base(context, supportedDatabaseTypes)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; }

        public override string ToString()
        {
            // Test for MySQL primary key situation.
            if (CurrentDatabaseType.IsMySql())
            {
                if (Constraint.IsPrimaryKeyConstraint)
                {
                    return string.Format(SqlSyntax.DeleteConstraint,
                                     SqlSyntax.GetQuotedTableName(Constraint.TableName),
                                     "PRIMARY KEY",
                                     "");
                }
                else
                {
                    return string.Format(SqlSyntax.DeleteConstraint,
                                     SqlSyntax.GetQuotedTableName(Constraint.TableName),
                                     "FOREIGN KEY",
                                     "");
                }
            }
            else
            {
                return string.Format(SqlSyntax.DeleteConstraint,
                                 SqlSyntax.GetQuotedTableName(Constraint.TableName),
                                 SqlSyntax.GetQuotedName(Constraint.ConstraintName));
            }
        }
    }
}