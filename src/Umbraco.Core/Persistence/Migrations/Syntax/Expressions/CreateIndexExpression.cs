using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateIndexExpression : MigrationExpressionBase
    {

        public CreateIndexExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes, IndexDefinition index)
            : base(context, supportedDatabaseTypes)
        {
            Index = index;
        }

        public CreateIndexExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        {
            Index = new IndexDefinition();
        }
        
        public IndexDefinition Index { get; set; }

        public override string ToString()
        {
            return SqlSyntax.Format(Index);
        }
    }
}