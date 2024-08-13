namespace Umbraco.Cms.Core.PropertyEditors;

public class MultiUrlPickerConfiguration : IIgnoreUserStartNodesConfig
{
    [ConfigurationField("minNumber")]
    public int MinNumber { get; set; }

    [ConfigurationField("maxNumber")]
    public int MaxNumber { get; set; }

    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
