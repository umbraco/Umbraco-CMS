using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Syntax.Expressions
{
    public class CreateForeignKeyExpression : MigrationExpressionBase
    {
        public CreateForeignKeyExpression()
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public CreateForeignKeyExpression(DatabaseProviders current, DatabaseProviders[] databaseProviders)
            : base(current, databaseProviders)
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public virtual ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlSyntaxContext.SqlSyntaxProvider.Format(ForeignKey);
        }
    }
}