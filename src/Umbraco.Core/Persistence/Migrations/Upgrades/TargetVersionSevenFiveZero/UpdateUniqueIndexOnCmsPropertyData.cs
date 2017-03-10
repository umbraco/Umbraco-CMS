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
                //Check for MySQL
                if (Context.CurrentDatabaseProvider == DatabaseProviders.MySql)
                {
                    //Use the special double nested sub query for MySQL since that is the only
                    //way delete sub queries works
                    var delPropQry = SqlSyntax.GetDeleteSubquery(
                        "cmsPropertyData",
                        "id",
                        new Sql("SELECT MIN(id) FROM cmsPropertyData GROUP BY contentNodeId, versionId, propertytypeid HAVING MIN(id) IS NOT NULL"),
                        WhereInType.NotIn);
                    Execute.Sql(delPropQry.SQL);
                }
                else
                {
                    //NOTE: Even though the above will work for MSSQL, we are not going to execute the
                    // nested delete sub query logic since it will be slower and there could be a ton of property 
                    // data here so needs to be as fast as possible.
                    Execute.Sql("DELETE FROM cmsPropertyData WHERE id NOT IN (SELECT MIN(id) FROM cmsPropertyData GROUP BY contentNodeId, versionId, propertytypeid HAVING MIN(id) IS NOT NULL)");
                }

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

        public override void Down()
        {
        }
    }
}