using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Delete.Expressions
{
    public class DeleteIndexExpression : IMigrationExpression
    {
        public DeleteIndexExpression()
        {
            Index = new IndexDefinition();
        }

        public virtual IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return string.Format(SyntaxConfig.SqlSyntaxProvider.DropIndex,
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedName(Index.Name),
                                 SyntaxConfig.SqlSyntaxProvider.GetQuotedTableName(Index.TableName));
        }
    }
}