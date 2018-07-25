using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenElevenTwelve
{
    /// <summary>
    /// Set the default storageType for the tags datatype to "CSV" to ensure backwards compatibilty since the default is going to be JSON in new versions
    /// </summary>
    
    [Migration("7.12.0", 1, Constants.System.UmbracoMigrationName)]
    public class SetDefaultTagsStorageType: MigrationBase
    {
        public SetDefaultTagsStorageType(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            if (Context == null || Context.Database == null) return;


            // We need to get all datatypes with an alias of "umbraco.tags" so we can loop over them and set the missing values if needed
            var nodeIds = Context.Database.Query<int>("SELECT NodeId FROM CmsDataType WHERE PropertyEditorAlias = {0}", Constants.PropertyEditors.TagsAlias);

            foreach (var nodeId in nodeIds)
            {
                // We need to check if the node has a "storageType" set
                var result = Context.Database.SingleOrDefault<string>("SELECT value FROM CmsDataTypePrevalue WHERE nodeId = {0} AND alias = '{1}'", nodeId, "storageType");

                // if the "storageType" has not been set we do so by adding a new row in the table for the nodid and set it
                if (result == null)
                {
                    Insert.IntoTable("CmsDataTypePrevalue").Row(new
                    {
                        datatypeNodId = nodeId,
                        value = "Csv",
                        sortOrder = 2,
                        alias = "storageType"
                    });
                }
            }
        }

        public override void Down()
        {
        }
    }
}
