using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{
    [Migration("7.3.0", 5, GlobalSettings.UmbracoMigrationName)]
    public class UpdateUniqueIdToHaveCorrectIndexType : MigrationBase
    {
        //see: http://issues.umbraco.org/issue/U4-6188, http://issues.umbraco.org/issue/U4-6187
        public override void Up()
        {


            var dbIndexes = SqlSyntax.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();

            //must be non-nullable
            Alter.Column("uniqueID").OnTable("umbracoNode").AsGuid().NotNullable();

            //make sure it already exists
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNodeUniqueID")))
            {
                Delete.Index("IX_umbracoNodeUniqueID").OnTable("umbracoNode");
            }
            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoNode_uniqueID")) == false)
            {
                //must be a uniqe index
                Create.Index("IX_umbracoNode_uniqueID").OnTable("umbracoNode").OnColumn("uniqueID").Unique();
            }

            
        }

        public override void Down()
        {
        }
    }
}