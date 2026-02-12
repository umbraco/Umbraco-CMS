namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides functionality to retrieve counts of nodes in the Umbraco content tree.
/// </summary>
public interface INodeCountService
{
    /// <summary>
    /// Gets the count of nodes of a specific object type.
    /// </summary>
    /// <param name="nodeType">The GUID representing the Umbraco object type.</param>
    /// <returns>The total count of nodes of the specified type.</returns>
    int GetNodeCount(Guid nodeType);

    /// <summary>
    /// Gets the total count of media items.
    /// </summary>
    /// <returns>The total count of media items in the system.</returns>
    int GetMediaCount();
}
