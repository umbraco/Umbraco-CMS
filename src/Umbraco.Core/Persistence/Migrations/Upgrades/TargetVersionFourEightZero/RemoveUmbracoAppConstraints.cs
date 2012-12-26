using System.Data;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourEightZero
{
    [MigrationAttribute("4.8.0", 0)]
    public class RemoveUmbracoAppConstraints : MigrationBase
    {
        public override void Up()
        {
            Delete.ForeignKey("FK_umbracoUser2app_umbracoApp").OnTable("umbracoUser2app");

            Delete.ForeignKey("FK_umbracoAppTree_umbracoApp").OnTable("umbracoAppTree");
        }

        public override void Down()
        {
            Create.ForeignKey("FK_umbracoUser2app_umbracoApp").FromTable("umbracoUser2app").ForeignColumn("app")
                .ToTable("umbracoApp").PrimaryColumn("appAlias").OnDeleteOrUpdate(Rule.None);

            Create.ForeignKey("FK_umbracoAppTree_umbracoApp").FromTable("umbracoAppTree").ForeignColumn("appAlias")
                .ToTable("umbracoApp").PrimaryColumn("appAlias").OnDeleteOrUpdate(Rule.None);
        }
    }
}