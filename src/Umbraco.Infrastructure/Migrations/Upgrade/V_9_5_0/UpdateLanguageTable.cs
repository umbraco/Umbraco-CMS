namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_5_0
{
    internal class UpdateLanguageTable : MigrationBase
    {
        public UpdateLanguageTable(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate() => Delete.Column("languageCultureName");
    }
}
