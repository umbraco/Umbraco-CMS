using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions
{
    public class AlterDefaultConstraintExpression : MigrationExpressionBase
    {
        public AlterDefaultConstraintExpression()
        {
        }

        public AlterDefaultConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }
        public virtual string ConstraintName { get; set; }
        public virtual object DefaultValue { get; set; }

        public override string ToString()
        {
            //NOTE Should probably investigate if Deleting a Default Constraint is different from deleting a 'regular' constraint

            return string.Format(SyntaxConfig.SqlSyntaxProvider.DeleteConstraint,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedName(ConstraintName));
        }
    }
}