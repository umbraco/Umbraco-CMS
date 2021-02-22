using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the grid value editor.
    /// </summary>
    public class GridConfiguration : IIgnoreUserStartNodesConfig
    {
        // TODO: Make these strongly typed, for now this works though
        [ConfigurationField("items", "Grid", "views/propertyeditors/grid/grid.prevalues.html", Description = "Grid configuration")]
        public JObject Items { get; set; }

        // TODO: Make these strongly typed, for now this works though
        [ConfigurationField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration", HideLabel = true)]
        public JObject Rte { get; set; }

        [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
            "Ignore User Start Nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }

        [ConfigurationField("mediaParentId", "Image Upload Folder", "mediafolderpicker",
            Description = "Choose the upload location of pasted images")]
        public GuidUdi MediaParentId { get; set; }
    }
}
