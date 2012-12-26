namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 7)]
    public class RenameTabIdColumn : MigrationBase
    {
        public override void Up()
        {
            Rename.Column("tabId").OnTable("cmsPropertyType").To("propertyTypeGroupId");
        }

        public override void Down()
        {
            Rename.Column("propertyTypeGroupId").OnTable("cmsPropertyType").To("tabId");
        }
    }
}