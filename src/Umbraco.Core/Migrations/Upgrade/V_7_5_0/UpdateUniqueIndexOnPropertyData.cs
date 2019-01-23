﻿using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_5_0
{
    /// <summary>
    /// See: http://issues.umbraco.org/issue/U4-8522
    /// </summary>
    public class UpdateUniqueIndexOnPropertyData : MigrationBase
    {
        public UpdateUniqueIndexOnPropertyData(IMigrationContext context)
            : base(context)
        {
        }

        public override void Migrate()
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
                Database.Execute("DELETE FROM cmsPropertyData WHERE id NOT IN (SELECT MIN(id) FROM cmsPropertyData GROUP BY nodeId, versionId, propertytypeid HAVING MIN(id) IS NOT NULL)");

                //we need to re create this index
                Delete.Index("IX_cmsPropertyData_1").OnTable("cmsPropertyData").Do();
                Create.Index("IX_cmsPropertyData_1").OnTable("cmsPropertyData")
                    .OnColumn("nodeId").Ascending()
                    .OnColumn("versionId").Ascending()
                    .OnColumn("propertytypeid").Ascending()
                    .WithOptions().NonClustered()
                    .WithOptions().Unique()
                    .Do();
            }
        }
    }
}
