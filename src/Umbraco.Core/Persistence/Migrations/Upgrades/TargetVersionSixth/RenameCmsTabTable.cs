namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 0)]
    public class RenameCmsTabTable : MigrationBase
    {
        public override void Up()
        {
            Rename.Table("cmsTab").To("cmsPropertyTypeGroup");
        }

        public override void Down()
        {
            Rename.Table("cmsPropertyTypeGroup").To("cmsTab");
        }
    }
}