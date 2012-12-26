namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 10)]
    public class DeleteAppTables : MigrationBase
    {
        public override void Up()
        {
            Delete.Table("umbracoAppTree");

            Delete.Table("umbracoApp");
        }

        public override void Down()
        {
        }
    }
}