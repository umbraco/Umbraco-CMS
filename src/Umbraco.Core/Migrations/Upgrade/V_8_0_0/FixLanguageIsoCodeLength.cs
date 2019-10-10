namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class FixLanguageIsoCodeLength : MigrationBase
    {
        public FixLanguageIsoCodeLength(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            // there is some confusion here when upgrading from v7
            // it should be 14 already but that's not always the case

            Alter.Table("umbracoLanguage")
                .AlterColumn("languageISOCode")
                .AsString(14)
                .Nullable()
                .Do();
        }
    }
}
