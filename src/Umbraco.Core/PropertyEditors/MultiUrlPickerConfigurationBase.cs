namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration shared by the URL picker property editors.
/// </summary>
public abstract class MultiUrlPickerConfigurationBase : IIgnoreUserStartNodesConfig
{
    /// <inheritdoc />
    [ConfigurationField(Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes)]
    public bool IgnoreUserStartNodes { get; set; }
}
