using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    
    [Migration("6.0.0", 10, GlobalSettings.UmbracoMigrationName)]
    public class DeleteAppTables : MigrationBase
    {
        public override void Up()
        {
            Delete.Table("umbracoAppTree");

            //NOTE: this is a hack since old umbraco versions might not have had their db's upgraded correctly so they are all quite inconsistent.
            // This is actually done in migration: RemoveUmbracoAppConstraints to target 4.9.0 but we've found with some db's that are currently at 4.9.1, 
            // these upgrades did not run. So, now we not only have to check if these constraints exist, but we also have to check if the RemoveUmbracoAppConstraints
            // has executed since we cannot drop the same foreign key twice or we'll get errors.

            //Here we'll do a dirty check to see if RemoveUmbracoAppConstraints has executed
            if (Context.Expressions.Any(x =>
            {
                var b = x as MigrationExpressionBase;
                if (b == null) return false;
                return b.Name == "FK_umbracoUser2app_umbracoApp";
            }) == false)
            {
                //These are the old aliases, before removing them, check they exist
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser2app") && x.Item3.InvariantEquals("FK_umbracoUser2app_umbracoApp")))
                {
                    Delete.ForeignKey("FK_umbracoUser2app_umbracoApp").OnTable("umbracoUser2app");
                }
                if (constraints.Any(x => x.Item1.InvariantEquals("umbracoUser2app") && x.Item3.InvariantEquals("FK_umbracoUser2app_umbracoUser")))
                {
                    Delete.ForeignKey("FK_umbracoUser2app_umbracoUser").OnTable("umbracoUser2app");
                }    
            }

            Delete.Table("umbracoApp");
        }

        public override void Down()
        {
            //This cannot be rolled back!!
            throw new DataLossException("Cannot rollback migration " + typeof(DeleteAppTables) + " the db tables umbracoAppTree and umbracoApp have been droppped");
        }
    }
}