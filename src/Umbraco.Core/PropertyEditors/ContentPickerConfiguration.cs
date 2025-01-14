namespace Umbraco.Cms.Core.PropertyEditors;

public class ContentPickerConfiguration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
