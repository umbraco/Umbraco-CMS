namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration for the element picker property editor.
/// </summary>
public class ElementPickerConfiguration : IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets the start node ID for the element picker.
    /// </summary>
    [ConfigurationField("startNodeId")]
    public Guid? StartNodeId { get; set; }

    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
