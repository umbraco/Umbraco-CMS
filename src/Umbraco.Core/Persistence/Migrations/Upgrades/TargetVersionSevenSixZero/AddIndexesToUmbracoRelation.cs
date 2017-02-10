using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddIndexesToUmbracoRelation : MigrationBase
    {
        public AddIndexesToUmbracoRelation(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            //Ensure this executes in a defered block which will be done inside of the migration transaction
            this.Execute.Code(database =>
            {
                var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(database);

                //make sure it doesn't already exist
                if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoRelation_parentChildType")) == false)
                {
                    //We need to check if this index has corrupted data and then clear that data
                    var duplicates = database.Fetch<dynamic>("SELECT parentId,childId,relType FROM umbracoRelation GROUP BY parentId,childId,relType HAVING COUNT(*) > 1");
                    if (duplicates.Count > 0)
                    {
                        //need to fix this there cannot be duplicates so we'll take the latest entries, it's really not going to matter though
                        foreach (var duplicate in duplicates)
                        {
                            var ids = database.Fetch<int>("SELECT id FROM umbracoRelation WHERE parentId=@parentId AND childId=@childId AND relType=@relType ORDER BY datetime DESC",
                                new { parentId = duplicate.parentId, childId = duplicate.childId, relType = duplicate.relType });

                            if (ids.Count == 1)
                            {
                                //this is just a safety check, this should absolutely never happen
                                throw new InvalidOperationException("Duplicates were detected but could not be discovered");
                            }

                            //delete the others
                            ids = ids.Skip(0).ToList();

                            //iterate in groups of 2000 to avoid the max sql parameter limit
                            foreach (var idGroup in ids.InGroupsOf(2000))
                            {
                                database.Execute("DELETE FROM umbracoRelation WHERE id IN (@ids)", new { ids = idGroup });
                            }
                        }
                    }
                }
                return "";
            });

            Create.Index("IX_umbracoRelation_parentChildType").OnTable("umbracoRelation")
                .OnColumn("parentId").Ascending()
                .OnColumn("childId").Ascending()
                .OnColumn("relType").Ascending()
                .WithOptions()
                .Unique();
        }

        public override void Down()
        {
            Delete.Index("IX_umbracoNodePath").OnTable("umbracoNode");
        }
    }
}