using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the multinode picker value editor.
/// </summary>
public class MultiNodePickerConfiguration : IIgnoreUserStartNodesConfig
{
    [JsonPropertyName("startNode")]
    [ConfigurationField("startNode")]
    public MultiNodePickerConfigurationTreeSource? TreeSource { get; set; }

    [ConfigurationField("minNumber")]
    public int MinNumber { get; set; }

    [ConfigurationField("maxNumber")]
    public int MaxNumber { get; set; }

    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
