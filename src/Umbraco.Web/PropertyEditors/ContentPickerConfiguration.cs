using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class ContentPickerConfiguration : IIgnoreUserStartNodesConfig
    {
        [ConfigurationField("showOpenButton", "Show open button", "boolean", Description = "Opens the node in a dialog")]
        public bool ShowOpenButton { get; set; }

        [ConfigurationField("startNodeId", "Start node", "treepicker")] // + config in configuration editor ctor
        public Udi StartNodeId { get; set; }

        [ConfigurationField(Core.Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
            "Ignore User Start Nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }
    }
}
