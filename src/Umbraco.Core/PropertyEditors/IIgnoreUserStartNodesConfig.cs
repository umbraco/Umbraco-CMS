namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Marker interface for any editor configuration that supports Ignoring user start nodes
/// </summary>
public interface IIgnoreUserStartNodesConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether to ignore user start nodes when selecting content.
    /// </summary>
    bool IgnoreUserStartNodes { get; set; }
}
