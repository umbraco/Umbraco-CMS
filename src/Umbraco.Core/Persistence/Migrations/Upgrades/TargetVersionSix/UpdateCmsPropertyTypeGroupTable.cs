using System.Data;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 1, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsPropertyTypeGroupTable : MigrationBase
    {
        public UpdateCmsPropertyTypeGroupTable(IMigrationContext context) 
            : base(context)
        { }


        public override void Up()
        {
            Alter.Table("cmsPropertyTypeGroup").AddColumn("parentGroupId").AsInt16().Nullable();

            Create.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup_id")
                .FromTable("cmsPropertyTypeGroup").ForeignColumn("parentGroupId")
                .ToTable("cmsPropertyTypeGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
        }

        public override void Down()
        {
            Delete.ForeignKey().FromTable("cmsPropertyTypeGroup").ForeignColumn("parentGroupId").ToTable("cmsPropertyTypeGroup").PrimaryColumn("id");

            Delete.Column("parentGroupId").FromTable("cmsPropertyTypeGroup");
        }
    }
}