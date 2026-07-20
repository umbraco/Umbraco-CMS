namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the multi URL picker property editor.
/// </summary>
public class MultiUrlPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the minimum number of URLs that must be selected.
    /// </summary>
    [ConfigurationField("minNumber")]
    public int MinNumber { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of URLs that can be selected.
    /// </summary>
    [ConfigurationField("maxNumber")]
    public int MaxNumber { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
