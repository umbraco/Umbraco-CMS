using NPoco;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Migrations.Expressions.Common.Expressions
{
    public class CreateForeignKeyExpression : MigrationExpressionBase
    {
        public CreateForeignKeyExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes, ForeignKeyDefinition fkDef)
            : base(context, supportedDatabaseTypes)
        {
            ForeignKey = fkDef;
        }

        public CreateForeignKeyExpression(IMigrationContext context, DatabaseType[] supportedDatabaseTypes)
            : base(context, supportedDatabaseTypes)
        {
            ForeignKey = new ForeignKeyDefinition();
        }

        public ForeignKeyDefinition ForeignKey { get; set; }

        public override string ToString() // fixme kill
            => GetSql();

        protected override string GetSql()
        {
            if (IsExpressionSupported() == false)
                return string.Empty;

            return SqlSyntax.Format(ForeignKey);
        }
    }
}
