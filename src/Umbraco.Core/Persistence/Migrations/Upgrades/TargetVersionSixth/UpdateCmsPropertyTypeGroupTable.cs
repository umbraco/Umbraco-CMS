using System.Data;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 1)]
    public class UpdateCmsPropertyTypeGroupTable : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("cmsPropertyTypeGroup").AddColumn("parentGroupId").AsInt16().Nullable();

            Create.UniqueConstraint("df_cmsPropertyTypeGroup_parentGroupId")
                .OnTable("cmsPropertyTypeGroup").Column("parentGroupId");

            Create.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup")
                .FromTable("cmsPropertyTypeGroup").ForeignColumn("parentGroupId")
                .ToTable("cmsPropertyTypeGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup").OnTable("cmsPropertyTypeGroup");

            Delete.UniqueConstraint("df_cmsPropertyTypeGroup_parentGroupId").FromTable("cmsPropertyTypeGroup");

            Delete.Column("parentGroupId").FromTable("cmsPropertyTypeGroup");
        }
    }
}