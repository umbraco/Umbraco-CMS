using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteTableExpression : MigrationExpressionBase
    {
        public DeleteTableExpression()
        {
        }

        public DeleteTableExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders) : base(current, databaseProviders)
        {
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.DropTable,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName));
        }
    }
}