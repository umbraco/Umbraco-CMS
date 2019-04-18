using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the grid value editor.
    /// </summary>
    public class GridConfiguration
    {
        // TODO: Make these strongly typed, for now this works though
        [ConfigurationField("items", "Grid", "views/propertyeditors/grid/grid.prevalues.html", Description = "Grid configuration")]
        public JObject Items { get; set; }

        // TODO: Make these strongly typed, for now this works though
        [ConfigurationField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration")]
        public JObject Rte { get; set; }

        [ConfigurationField("ignoreUserStartNodes", "Ignore user start nodes", "boolean", Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.<br /> <em>Note: this applies to all editors in this grid editor except for the rich text editor, which has it's own option for that.</em>")]
        public bool IgnoreUserStartNodes { get; set; }
    }
}
