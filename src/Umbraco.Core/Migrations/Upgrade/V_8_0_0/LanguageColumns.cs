namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class LanguageColumns : MigrationBase
    {
        protected LanguageColumns(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            Create.Column("isDefaultVariantLang").OnTable(Constants.DatabaseSchema.Tables.Language).AsBoolean().NotNullable();
            Create.Column("mandatory").OnTable(Constants.DatabaseSchema.Tables.Language).AsBoolean().NotNullable();
        }
    }
}
