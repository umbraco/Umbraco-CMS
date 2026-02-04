namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the content picker property editor.
/// </summary>
public class ContentPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
