using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for the multinode picker value editor.
/// </summary>
public class MultiNodePickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the tree source configuration for the picker.
    /// </summary>
    [JsonPropertyName("startNode")]
    [ConfigurationField("startNode")]
    public MultiNodePickerConfigurationTreeSource? TreeSource { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of items that must be selected.
    /// </summary>
    [ConfigurationField("minNumber")]
    public int MinNumber { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of items that can be selected.
    /// </summary>
    [ConfigurationField("maxNumber")]
    public int MaxNumber { get; set; }

    /// <summary>
    /// Gets or sets the content type filter for allowed selections.
    /// </summary>
    [ConfigurationField("filter")]
    public string? Filter { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
