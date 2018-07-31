namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{
    public class IncreaseLanguageIsoCodeColumnLength : MigrationBase
    {
        public IncreaseLanguageIsoCodeColumnLength(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            Delete.Index("IX_umbracoLanguage_languageISOCode").OnTable("umbracoLanguage").Do();

            Alter.Table("umbracoLanguage")
                .AlterColumn("languageISOCode")
                .AsString(14)
                .Nullable()
                .Do();

            Create.Index("IX_umbracoLanguage_languageISOCode")
                .OnTable("umbracoLanguage")
                .OnColumn("languageISOCode")
                .Unique()
                .Do();
        }
    }
}
