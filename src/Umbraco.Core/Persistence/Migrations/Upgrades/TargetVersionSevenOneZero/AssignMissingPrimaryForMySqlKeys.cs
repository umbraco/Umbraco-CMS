using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenOneZero
{
    //see: http://issues.umbraco.org/issue/U4-4430 - Except this one is specific to 7 because
    // we've modified the tagRelationship PK already.
    //We have to target this specifically however to ensure this DOES NOT execute if upgrading from a version previous to 7.0,
    // this is because when the 7.0.0 migrations are executed, this primary key get's created so if this migration is also executed
    // we will get exceptions because it is trying to create the PK two times.

    [Migration("7.0.0", "7.1.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AssignMissingPrimaryForMySqlKeys : MigrationBase
    {
        public override void Up()
        {
            if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
            {
                var constraints = SqlSyntax.GetConstraintsPerColumn(Context.Database).Distinct().ToArray();
                
                //This should be 3 because this table has 3 keys
                if (constraints.Count(x => x.Item1.InvariantEquals("cmsTagRelationship") && x.Item3.InvariantEquals("PRIMARY")) == 0)
                {
                    Create.PrimaryKey("PK_cmsTagRelationship")
                        .OnTable("cmsTagRelationship")
                        .Columns(new[] { "nodeId", "propertyTypeId", "tagId" });
                }

            }
        }

        public override void Down()
        {
            //don't do anything, these keys should have always existed!
        }
    }
}