using System.Data;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionFourNineZero
{
    [MigrationAttribute("4.9.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class RemoveUmbracoAppConstraints : MigrationBase
    {
        public override void Up()
        {
            Delete.ForeignKey().FromTable("umbracoUser2app").ForeignColumn("app").ToTable("umbracoApp").PrimaryColumn("appAlias");

            Delete.ForeignKey().FromTable("umbracoAppTree").ForeignColumn("appAlias").ToTable("umbracoApp").PrimaryColumn("appAlias");
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