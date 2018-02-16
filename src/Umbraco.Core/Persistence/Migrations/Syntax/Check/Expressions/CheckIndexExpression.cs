using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Check.Expressions
{
    public class CheckIndexExpression : MigrationExpressionBase
    {
        public CheckIndexExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) : base(current, databaseProviders, sqlSyntax)
        {
        }

        public string IndexName { get; set; }
        public string TableName { get; set; }
        public string[] ColumnNames { get; set; }
        public bool? Unique { get; set; }
    }
}
