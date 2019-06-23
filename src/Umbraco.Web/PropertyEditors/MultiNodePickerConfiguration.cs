using Newtonsoft.Json.Linq;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the multinode picker value editor.
    /// </summary>
    public class MultiNodePickerConfiguration
    {
        [ConfigurationField("startNode", "Node type", "treesource")]
        public MultiNodePickerConfigurationTreeSource TreeSource { get; set; }

        [ConfigurationField("filter", "Allow items of type", "contenttypepicker", Description = "Select the applicable content types")]
        public string Filter { get; set; }

        [ConfigurationField("minNumber", "Minimum number of items", "number")]
        public int MinNumber { get; set; }

        [ConfigurationField("maxNumber", "Maximum number of items", "number")]
        public int MaxNumber { get; set; }

        [ConfigurationField("showOpenButton", "Show open button (this feature is in preview!)", "boolean", Description = "Opens the node in a dialog")]
        public bool ShowOpen { get; set; }
    }
}
