using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckColumnsExpression : MigrationExpressionBase
    {
        public CheckColumnsExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
        }

        public string[] ColumnNames { get; set; }
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
    }
}
