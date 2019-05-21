using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public abstract class BlockEditorConfiguration
    {
        [ConfigurationField("blocks", "Blocks", "views/propertyeditors/blockeditor/blockeditor.settings.html")]
        public Block[] Blocks { get; set; }

        [ConfigurationField("view", "View", "textstring", Description = "The path to a custom view for rendering the editor")]
        public string View { get; set; }

        public class Block
        {
            [JsonProperty("elementType")]
            public Udi ElementType { get; set; }

            [JsonProperty("settings")]
            public BlockSetting Settings { get; set; }
        }

        public class BlockSetting
        {
            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("dataType")]
            public Udi DataType { get; set; }
        }
    }
}
