using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteTableExpression : IMigrationExpression
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.DropTable,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(TableName));
        }
    }
}