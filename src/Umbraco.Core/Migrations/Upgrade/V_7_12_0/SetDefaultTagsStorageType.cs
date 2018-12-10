using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_7_12_0
{
    /// <summary>
    /// Set the default storageType for the tags datatype to "CSV" to ensure backwards compatibility since the default is going to be JSON in new versions.
    /// </summary>
    public class SetDefaultTagsStorageType : MigrationBase
    {
        public SetDefaultTagsStorageType(IMigrationContext context)
            : base(context)
        { }

        // dummy editor for deserialization
        private class TagConfigurationEditor : ConfigurationEditor<TagConfiguration>
        { }

        public override void Migrate()
        {
            // get all Umbraco.Tags datatypes
            var dataTypeDtos = Database.Fetch<DataTypeDto>(Context.SqlContext.Sql()
                .Select<DataTypeDto>()
                .From<DataTypeDto>()
                .Where<DataTypeDto>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.Tags));

            // get a dummy editor for deserialization
            var editor = new TagConfigurationEditor();

            foreach (var dataTypeDto in dataTypeDtos)
            {
                // need to check storageType on raw dictionary, as TagConfiguration would have a default value
                var dictionary = JsonConvert.DeserializeObject<JObject>(dataTypeDto.Configuration);

                // if missing, use TagConfiguration to properly update the configuration
                // due to ... reasons ... the key can start with a lower or upper 'S'
                if (!dictionary.ContainsKey("storageType") && !dictionary.ContainsKey("StorageType"))
                {
                    var configuration = (TagConfiguration)editor.FromDatabase(dataTypeDto.Configuration);
                    configuration.StorageType = TagsStorageType.Csv;
                    dataTypeDto.Configuration = ConfigurationEditor.ToDatabase(configuration);
                    Database.Update(dataTypeDto);
                }
            }
        }
    }
}
