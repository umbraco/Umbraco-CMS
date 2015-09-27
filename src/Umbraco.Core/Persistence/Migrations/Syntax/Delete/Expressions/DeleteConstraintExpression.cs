using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteConstraintExpression : MigrationExpressionBase
    {
        
        public DeleteConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax, ConstraintType type)
            : base(current, databaseProviders, sqlSyntax)
        {
            Constraint = new ConstraintDefinition(type);
        }

        public ConstraintDefinition Constraint { get; private set; }

        public override string ToString()
        {
            // Test for MySQL primary key situation.
            if (CurrentDatabaseProvider == DatabaseProviders.MySql)
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