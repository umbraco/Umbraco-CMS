using System;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenTwelveZero
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
            if (Context?.Database == null) return;
            
            // We need to get all datatypes with an alias of "umbraco.tags" so we can loop over them and set the missing values if needed
            var datatypes = Context.Database.Fetch<DataTypeDto>("SELECT * FROM cmsDataType");
            var tagsDataTypes = datatypes.Where(x => string.Equals(x.PropertyEditorAlias, Constants.PropertyEditors.TagsAlias, StringComparison.InvariantCultureIgnoreCase));
            var dataTypePreValues = Context.Database.Fetch<DataTypePreValueDto>("SELECT * FROM cmsDataTypePreValues");

            foreach (var datatype in tagsDataTypes)
            {
                // We need to check if the node has a "storageType" set
                var result = dataTypePreValues.FirstOrDefault(x =>
                    x.DataTypeNodeId == datatype.DataTypeId
                    && string.Equals(x.Alias, "storageType", StringComparison.InvariantCultureIgnoreCase));

                // if the "storageType" has not been set we do so by adding a new row in the table for the nodid and set it
                if (result == null)
                {
                    Insert.IntoTable("cmsDataTypePreValues").Row(new
                    {
                        datatypeNodeId = datatype.DataTypeId,
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
