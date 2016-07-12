using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFiveZero
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-8522
    /// </summary>
    [Migration("7.5.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class UpdateUniqueIndexOnCmsPropertyData : MigrationBase
    {
        public UpdateUniqueIndexOnCmsPropertyData(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            //Clear all stylesheet data if the tables exist
            //tuple = tablename, indexname, columnname, unique
            var indexes = SqlSyntax.GetDefinedIndexes(Context.Database).ToArray();
            var found = indexes.FirstOrDefault(
                x => x.Item1.InvariantEquals("cmsPropertyData")
                     && x.Item2.InvariantEquals("IX_cmsPropertyData_1")
                    //we're searching for the old index which is not unique
                     && x.Item4 == false);

            if (found != null)
            {
                ////NOTE: WE cannot execute this SQL inside of an Execute block because even though we are removing duplicates,
                ////we cannot drop/add an index until that trans is completed, so we need to do this eagerly.

                //In order to apply this unique index, we must ensure that there is no duplicate data.
                // so we need to query for this
                Execute.Code(database =>
                {
                    const string sql = @"SELECT mt.id, mt.contentNodeId, mt.versionId, mt.propertytypeId FROM cmsPropertyData mt
INNER JOIN (
    SELECT contentNodeId, versionId, propertytypeId
    FROM cmsPropertyData
    GROUP BY contentNodeId, versionId, propertytypeId
    HAVING COUNT(*) >1
) T 
ON mt.contentNodeId= T.contentNodeId
and mt.versionId= T.versionId
and mt.propertytypeId= T.propertytypeId
ORDER BY mt.id";

                    var duplicates = database.Query<dynamic>(sql);
                    var currDuplicates = new List<Tuple<int, int, Guid, int>>();
                    foreach (var duplicate in duplicates)
                    {
                        //check if the current duplicates batch is changing as we iterate
                        if (currDuplicates.Count > 0                        
                            && currDuplicates[0].Item2 == duplicate.contentNodeId
                            && currDuplicates[0].Item3 == duplicate.versionId
                            && currDuplicates[0].Item4 == duplicate.propertytypeId)
                        {
                            //this is still a duplicate so add it
                            AddDuplicate(duplicate, currDuplicates);
                        }
                        else
                        {
                            //the batch is changing so perform the deletions except for last
                            for (var index = 0; index < (currDuplicates.Count - 1); index++)
                            {
                                var currDuplicate = currDuplicates[index];
                                database.Delete<PropertyTypeDto>(currDuplicate.Item1);
                            }
                            currDuplicates.Clear();
                            //add the next batch
                            AddDuplicate(duplicate, currDuplicates);
                        }
                    }

                    //now we need to process the last batch
                    for (var index = 0; index < (currDuplicates.Count - 1); index++)
                    {
                        var currDuplicate = currDuplicates[index];
                        var result = database.Delete<PropertyDataDto>(currDuplicate.Item1);
                        Debug.Assert(result == 1);
                    }
                    currDuplicates.Clear();

                    return string.Empty;
                });

                //we need to re create this index
                Delete.Index("IX_cmsPropertyData_1").OnTable("cmsPropertyData");
                Create.Index("IX_cmsPropertyData_1").OnTable("cmsPropertyData")
                    .OnColumn("contentNodeId").Ascending()
                    .OnColumn("versionId").Ascending()
                    .OnColumn("propertytypeid").Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique();
            }
        }

        private void AddDuplicate(dynamic duplicate, List<Tuple<int, int, Guid, int>> list)
        {
            list.Add(new Tuple<int, int, Guid, int>(duplicate.id, duplicate.contentNodeId, duplicate.versionId, duplicate.propertytypeId));
        }

        public override void Down()
        {
        }
    }
}