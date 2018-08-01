using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{
    /// <summary>
    /// Set the default storageType for the tags datatype to "CSV" to ensure backwards compatibilty since the default is going to be JSON in new versions
    /// </summary>    
    public class SetDefaultTagsStorageType : MigrationBase
    {
        public SetDefaultTagsStorageType(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            if (Context?.Database == null) return;

            // We need to get all datatypes with an alias of "umbraco.tags" so we can loop over them and set the missing values if needed
            var datatypes = Context.Database.Fetch<DataTypeDto>();
            var tagsDataTypes = datatypes.Where(x => string.Equals(x.EditorAlias, Constants.PropertyEditors.Aliases.Tags, StringComparison.InvariantCultureIgnoreCase));

            foreach (var datatype in tagsDataTypes)
            {
                var dataTypePreValues = JsonConvert.DeserializeObject<JObject>(datatype.Configuration);

                // We need to check if the node has a "storageType" set
                if (!dataTypePreValues.ContainsKey("storageType"))
                {
                    dataTypePreValues["storageType"] = "Csv";
                }

                Update.Table(Constants.DatabaseSchema.Tables.DataType)
                    .Set(new { config = JsonConvert.SerializeObject(dataTypePreValues) })
                    .Where(new { nodeId = datatype.NodeId })
                    .Do();
            }
        }


    }
}
