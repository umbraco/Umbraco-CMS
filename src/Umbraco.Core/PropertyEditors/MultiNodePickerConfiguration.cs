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
    /// Gets or sets the validation limits for the number of items that can be selected.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum number of items that must be selected.
    /// </summary>
    [Obsolete("Use ValidationLimit instead. Scheduled for removal in Umbraco 21.")]
    [JsonIgnore]
    public int MinNumber
    {
        get => ValidationLimit.Min ?? 0;
        set => ValidationLimit.Min = value == 0 ? null : value;
    }

    /// <summary>
    /// Gets or sets the maximum number of items that can be selected.
    /// </summary>
    [Obsolete("Use ValidationLimit instead. Scheduled for removal in Umbraco 21.")]
    [JsonIgnore]
    public int MaxNumber
    {
        get => ValidationLimit.Max ?? 0;
        set => ValidationLimit.Max = value == 0 ? null : value;
    }

    /// <summary>
    /// Gets or sets the content type filter for allowed selections.
    /// </summary>
    [ConfigurationField("filter")]
    public string? Filter { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
