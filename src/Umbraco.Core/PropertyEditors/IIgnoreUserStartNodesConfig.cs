using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Marker interface for any editor configuration that supports Ignoring user start nodes
    /// </summary>
    [UmbracoVolatile]
    public interface IIgnoreUserStartNodesConfig
    {
        bool IgnoreUserStartNodes { get; set; }
    }
}
