using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{    
    //We have to target this specifically to ensure this DOES NOT execute if upgrading from a version previous to 6.0,
    // this is because when the 6.0.0 migrations are executed, this primary key get's created so if this migration is also executed
    // we will get exceptions because it is trying to create the PK two times.
    [Migration("6.0.0", "6.2.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AssignMissingPrimaryForMySqlKeys2 : MigrationBase
    {
        public override void Up()
        {
            if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();

                //This should be 2 because this table has 2 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsContentType2ContentType") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_cmsContentType2ContentType")
                        .OnTable("cmsContentType2ContentType")
                        .Columns(new[] {"parentContentTypeId", "childContentTypeId"});
                }
            }
        }

        public override void Down()
        {
            //don't do anything, these keys should have always existed!
        }
    }
}