using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the configuration for a multiple textstring value editor.
/// </summary>
public class MultipleTextStringConfiguration
{
    /// <summary>
    /// Gets or sets the validation limits for the number of text strings allowed.
    /// </summary>
    [ConfigurationField("validationLimit", Type = typeof(RangeConfigurationField))]
    public NumberRange ValidationLimit { get; set; } = new();

    /// <summary>
    /// Gets or sets the minimum number of text strings required.
    /// </summary>
    [Obsolete("Use ValidationLimit instead. Scheduled for removal in Umbraco 21.")]
    [JsonIgnore]
    public int Min
    {
        get => ValidationLimit.Min ?? 0;
        set => ValidationLimit.Min = value == 0 ? null : value;
    }

    /// <summary>
    /// Gets or sets the maximum number of text strings allowed.
    /// </summary>
    [Obsolete("Use ValidationLimit instead. Scheduled for removal in Umbraco 21.")]
    [JsonIgnore]
    public int Max
    {
        get => ValidationLimit.Max ?? 0;
        set => ValidationLimit.Max = value == 0 ? null : value;
    }
}
