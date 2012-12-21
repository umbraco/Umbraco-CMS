using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateForeignKeyExpression : IMigrationExpression
    {
        public CreateForeignKeyExpression()
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            return SyntaxConfig.SqlSyntaxProvider.Format(ForeignKey);
        }
    }
}