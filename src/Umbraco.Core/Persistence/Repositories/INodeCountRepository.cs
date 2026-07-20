namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for counting nodes in the system.
/// </summary>
public interface INodeCountRepository
{
    /// <summary>
    ///     Gets the count of nodes for a specific node type.
    /// </summary>
    /// <param name="nodeType">The unique identifier of the node type.</param>
    /// <returns>The count of nodes of the specified type.</returns>
    int GetNodeCount(Guid nodeType);

    /// <summary>
    ///     Gets the total count of media items.
    /// </summary>
    /// <returns>The count of media items.</returns>
    int GetMediaCount();
}
