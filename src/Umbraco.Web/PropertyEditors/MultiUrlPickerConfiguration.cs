using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    public class MultiUrlPickerConfiguration
    {
        [ConfigurationField("minNumber", "Minimum number of items", "number")]
        public int MinNumber { get; set; }

        [ConfigurationField("maxNumber", "Maximum number of items", "number")]
        public int MaxNumber { get; set; }

        [ConfigurationField("ignoreUserStartNodes", "Ignore user start nodes", "boolean",
            Description = "Selecting this option allows a user to choose nodes that they normally don't have access to.")]
        public bool IgnoreUserStartNodes { get; set; }
       
    }
}
