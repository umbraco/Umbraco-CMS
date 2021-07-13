using Newtonsoft.Json;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    public class MultiUrlPickerConfiguration : IIgnoreUserStartNodesConfig
    {
        [ConfigurationField("validationLimit", "Amount", "numberrange", Description = "Set a required range of urls")]
        public NumberRange ValidationLimit { get; set; } = new NumberRange();

        public class NumberRange
        {
            [JsonProperty("min")]
            public int? Min { get; set; }

            [JsonProperty("max")]
            public int? Max { get; set; }
        }

        [ConfigurationField(Core.Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes,
            "Ignore User Start Nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }

        [ConfigurationField("hideAnchor",
            "Hide anchor/query string input", "boolean",
            Description = "Selecting this hides the anchor/query string input field in the linkpicker overlay.")]
        public bool HideAnchor { get; set; }
    }
}
