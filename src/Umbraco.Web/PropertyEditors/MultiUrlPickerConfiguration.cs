using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{

    public class MultiUrlPickerConfiguration : IIgnoreUserStartNodesConfig
    {
        [ConfigurationField("minNumber", "Minimum number of items", "number")]
        public int MinNumber { get; set; }

        [ConfigurationField("maxNumber", "Maximum number of items", "number")]
        public int MaxNumber { get; set; }

        [ConfigurationField(Core.Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes, "Selecting this option allows a user to choose nodes that they normally don't have access to.", "boolean")]
        public bool IgnoreUserStartNodes { get; set; }

        
    }
}
