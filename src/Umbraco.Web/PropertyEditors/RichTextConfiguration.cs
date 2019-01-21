using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the rich text value editor.
    /// </summary>
    public class RichTextConfiguration
    {
        //todo: Make these strongly typed, for now this works though
        [ConfigurationField("editor", "Editor", "views/propertyeditors/rte/rte.prevalues.html", HideLabel = true)]
        public JObject Editor { get; set; }

        [ConfigurationField("hideLabel", "Hide Label", "boolean")]
        public bool HideLabel { get; set; }
    }
}
