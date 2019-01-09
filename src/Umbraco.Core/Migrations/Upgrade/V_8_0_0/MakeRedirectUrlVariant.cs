using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class MakeRedirectUrlVariant : MigrationBase
    {
        public MakeRedirectUrlVariant(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            AddColumn<RedirectUrlDto>("culture");

            Delete.Index("IX_umbracoRedirectUrl").OnTable(Constants.DatabaseSchema.Tables.RedirectUrl).Do();
            Create.Index("IX_umbracoRedirectUrl").OnTable(Constants.DatabaseSchema.Tables.RedirectUrl)
                .OnColumn("urlHash")
                .Ascending()
                .OnColumn("contentKey")
                .Ascending()
                .OnColumn("culture")
                .Ascending()
                .OnColumn("createDateUtc")
                .Ascending()
                .WithOptions().Unique()
                .Do();
        }
    }
}
