using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// The configuration object for the Block List editor
    /// </summary>
    public class BlockListConfiguration
    {

        // TODO: rename this to blockDefinitions, cause its not elementTypes, its a dictionary of objects that define blocks, part of a block is the elementType used as content model.
        [ConfigurationField("elementTypes", "Available Blocks", "views/propertyeditors/blocklist/prevalue/blocklist.elementtypepicker.html", Description = "Define the available blocks.")]
        public ElementType[] ElementTypes { get; set; }


        [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of blocks")]
        public NumberRange ValidationLimit { get; set; } = new NumberRange();

        public class NumberRange
        {
            [JsonProperty("min")]
            public int? Min { get; set; }

            [JsonProperty("max")]
            public int? Max { get; set; }
        }

        public class ElementType
        {
            // TODO: rename this to contentElementTypeAlias, I would like this to be specific, since we have the settings.
            [JsonProperty("elementTypeAlias")]
            public string Alias { get; set; }

            [JsonProperty("settingsElementTypeAlias")]
            public string SettingsElementTypeAlias { get; set; }

            [JsonProperty("view")]
            public string View { get; set; }

            [JsonProperty("labelTemplate")]
            public string Template { get; set; }
        }

        [ConfigurationField("useAccordionsAsDefault", "Inline editing mode", "boolean", Description = "Use the inline editor as the default block view")]
        public bool useInlineEditingAsDefault { get; set; }

    }
}
