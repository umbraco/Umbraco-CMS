namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the element picker property editor.
/// </summary>
public class ElementPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
