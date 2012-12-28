using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteDefaultConstraintExpression : MigrationExpressionBase
    {
        public DeleteDefaultConstraintExpression()
        {
        }

        public DeleteDefaultConstraintExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : base(current, databaseProviders)
        {
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return string.Format(SyntaxConfig.SqlSyntaxProvider.DeleteDefaultConstraint,
                                 TableName,
                                 ColumnName);
        }
    }
}