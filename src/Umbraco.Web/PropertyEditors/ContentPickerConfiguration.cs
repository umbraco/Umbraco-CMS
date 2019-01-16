using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class ContentPickerConfiguration
    {
        [ConfigurationField("showOpenButton", "Show open button (this feature is in beta!)", "boolean", Description = "Opens the node in a dialog")]
        public bool ShowOpenButton { get; set; }

        [ConfigurationField("startNodeId", "Start node", "treepicker")] // + config in configuration editor ctor
        public string StartNodeId { get; set; }
    }
}
