using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    /// <summary>
    /// The configuration object for the Block List editor
    /// </summary>
    public class BlockListConfiguration
    {
        [ConfigurationField("elementTypes", "Element Types", "views/propertyeditors/blocklist/blocklist.elementtypepicker.html", Description = "Select the Element Types to use as models for the items.")]
        public ElementType[] ElementTypes { get; set; }

        // TODO: Fill me in

        public class ElementType
        {
            [JsonProperty("elementTypeAlias")]
            public string Alias { get; set; }
        }

    }
}
