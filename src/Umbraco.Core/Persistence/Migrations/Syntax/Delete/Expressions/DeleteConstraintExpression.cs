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

        public DeleteConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ConstraintType type)
            : base(current, databaseProviders)
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
                    return string.Format(SqlSyntaxContext.SqlSyntaxProvider.DeleteConstraint,
                                     SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(Constraint.TableName),
                                     "PRIMARY KEY",
                                     "");
                }
                else
                {
                    return string.Format(SqlSyntaxContext.SqlSyntaxProvider.DeleteConstraint,
                                     SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(Constraint.TableName),
                                     "FOREIGN KEY",
                                     "");
                }
            }
            else
            {
                return string.Format(SqlSyntaxContext.SqlSyntaxProvider.DeleteConstraint,
                                 SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(Constraint.TableName),
                                 SqlSyntaxContext.SqlSyntaxProvider.GetQuotedName(Constraint.ConstraintName));
            }
        }
    }
}