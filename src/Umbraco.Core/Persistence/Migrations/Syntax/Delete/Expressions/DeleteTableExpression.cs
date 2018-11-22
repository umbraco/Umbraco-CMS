using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteTableExpression : MigrationExpressionBase
    {
        
        public DeleteTableExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders, ISqlSyntaxProvider sqlSyntax) 
            : base(current, databaseProviders, sqlSyntax)
        {
        }

        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return string.Format(SqlSyntax.DropTable,
                                 SqlSyntax.GetQuotedTableName(TableName));
        }
    }
}