namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Marker interface for any editor configuration that supports Ignoring user start nodes
/// </summary>
public interface IIgnoreUserStartNodesConfig
{
    bool IgnoreUserStartNodes { get; set; }
}
