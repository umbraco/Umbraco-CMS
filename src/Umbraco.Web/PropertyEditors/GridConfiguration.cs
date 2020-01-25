using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.PropertyEditorsConfigs;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the grid value editor.
    /// </summary>
    public class GridConfiguration : IIgnoreUserStartNodesConfig
    {
        [ConfigurationField("items", "Grid", "views/propertyeditors/grid/grid.prevalues.html", Description = "Grid configuration")]
        public GridItems Items { get; set; }

        [ConfigurationField("rte", "Rich text editor", "views/propertyeditors/rte/rte.prevalues.html", Description = "Rich text editor configuration", HideLabel = true)]
        public RichTextSettings RichText { get; set; }

        [ConfigurationField(Core.Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
            "Ignore User Start Nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }

        [ConfigurationField("mediaParentId", "Image Upload Folder", "MediaFolderPicker",
            Description = "Choose the upload location of pasted images")]
        public GuidUdi MediaParentId { get; set; }
    }
}
