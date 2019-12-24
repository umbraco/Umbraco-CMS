using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Common.Expressions
{
    public class CreateForeignKeyExpression : MigrationExpressionBase
    {
        public CreateForeignKeyExpression(IMigrationContext context, ForeignKeyDefinition fkDef)
            : base(context)
        {
            ForeignKey = fkDef;
        }

        public CreateForeignKeyExpression(IMigrationContext context)
            : base(context)
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public ForeignKeyDefinition ForeignKey { get; set; }

        protected override string GetSql()
        {
            return SqlSyntax.Format(ForeignKey);
        }
    }
}
