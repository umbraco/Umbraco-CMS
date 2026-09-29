using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the multi URL picker property editor.
/// </summary>
public class MultiUrlPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the validation limits for the number of URLs that can be selected.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum number of URLs that must be selected.
    /// </summary>
    [Obsolete("Use ValidationLimit instead. Scheduled for removal in Umbraco 21.")]
    [JsonIgnore]
    public int MinNumber
    {
        get => ValidationLimit.Min ?? 0;
        set => ValidationLimit.Min = value == 0 ? null : value;
    }

    /// <summary>
    /// Gets or sets the maximum number of URLs that can be selected.
    /// </summary>
    [Obsolete("Use ValidationLimit instead. Scheduled for removal in Umbraco 21.")]
    [JsonIgnore]
    public int MaxNumber
    {
        get => ValidationLimit.Max ?? 0;
        set => ValidationLimit.Max = value == 0 ? null : value;
    }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
