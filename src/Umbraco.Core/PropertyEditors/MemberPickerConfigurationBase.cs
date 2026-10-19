namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents the configuration shared by the member picker property editors.
/// </summary>
/// <remarks>
/// Members are not part of a tree a user can be given a start node in, so - unlike the document, media and element
/// pickers - a member picker has nothing to ignore and does not implement <see cref="IIgnoreUserStartNodesConfig" />.
/// </remarks>
public abstract class MemberPickerConfigurationBase
{
    /// <summary>
    /// Gets or sets the member type filter for allowed selections.
    /// </summary>
    [ConfigurationField("filter")]
    public string? Filter { get; set; }
}
