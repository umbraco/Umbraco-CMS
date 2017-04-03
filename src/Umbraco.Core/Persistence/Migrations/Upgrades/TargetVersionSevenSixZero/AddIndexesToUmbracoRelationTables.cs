using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenSixZero
{
    [Migration("7.6.0", 0, Constants.System.UmbracoMigrationName)]
    public class AddIndexesToUmbracoRelationTables : MigrationBase
    {
        public AddIndexesToUmbracoRelationTables(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database).ToArray();

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoRelation_parentChildType")) == false)
            {
                //This will remove any corrupt/duplicate data in the relation table before the index is applied
                //Ensure this executes in a defered block which will be done inside of the migration transaction
                this.Execute.Code(database =>
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
                    return "";
                });

                //unique index to prevent duplicates - and for better perf
                Create.Index("IX_umbracoRelation_parentChildType").OnTable("umbracoRelation")
                    .OnColumn("parentId").Ascending()
                    .OnColumn("childId").Ascending()
                    .OnColumn("relType").Ascending()
                    .WithOptions()
                    .Unique();
            }

            //need indexes on alias and name for relation type since these are queried against

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoRelationType_alias")) == false)
            {                
                Create.Index("IX_umbracoRelationType_alias").OnTable("umbracoRelationType")
                    .OnColumn("alias")
                    .Ascending()
                    .WithOptions()
                    .NonClustered();
            }
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_umbracoRelationType_name")) == false)
            {
                Create.Index("IX_umbracoRelationType_name").OnTable("umbracoRelationType")
                    .OnColumn("name")
                    .Ascending()
                    .WithOptions()
                    .NonClustered();
            }

        }

        public override void Down()
        {
            Delete.Index("IX_umbracoNodePath").OnTable("umbracoNode");
        }
    }
}