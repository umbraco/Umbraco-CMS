using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    
    [Migration("6.0.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class DeleteAppTables : MigrationBase
    {
        public override void Up()
        {
            Delete.Table("umbracoAppTree");

            Delete.Table("umbracoApp");
        }

        public override void Down()
        {
            //This cannot be rolled back!!
            throw new DataLossException("Cannot rollback migration " + typeof(DeleteAppTables) + " the db tables umbracoAppTree and umbracoApp have been droppped");
        }
    }
}