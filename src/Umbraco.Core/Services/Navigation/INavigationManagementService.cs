namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for managing the navigation structure.
/// </summary>
public interface INavigationManagementService
{
    /// <summary>
    ///     Rebuilds the entire navigation structure by refreshing the navigation tree based
    ///     on the current state of the underlying repository.
    /// </summary>
    Task RebuildAsync();

    /// <summary>
    ///     Removes a node from the main navigation structure and moves it, along with
    ///     its descendants, to the root of the recycle bin structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node to remove.</param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully removed from the
    ///     main navigation structure and added to the recycle bin; otherwise, <c>false</c>.
    /// </returns>
    bool MoveToBin(Guid key);

    /// <summary>
    ///     Adds a new node to the main navigation structure. If a parent key is provided,
    ///     the new node is added as a child of the specified parent. If no parent key is
    ///     provided, the new node is added at the root level.
    /// </summary>
    /// <param name="key">The unique identifier of the new node to add.</param>
    /// <param name="contentTypeKey">The unique identifier of the node's content type.</param>
    /// <param name="parentKey">
    ///     The unique identifier of the parent node. If <c>null</c>, the new node will be added to
    ///     the root level.
    /// </param>
    /// <param name="sortOrder">
    ///     Optional value to define the node's position among its siblings when
    ///     adding node at root level.</param>
    /// <returns>
    ///     <c>true</c> if the node was successfully added to the main navigation structure;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     The sort order is particularly important when adding nodes at the root level. For child nodes,
    ///     it can usually be determined by the number of existing children under the parent. However,
    ///     when adding nodes directly to the root (where parentKey is null), a sort order must be provided
    ///     to ensure the item appears in the correct position among other root-level items.
    /// </remarks>
    bool Add(Guid key, Guid contentTypeKey, Guid? parentKey = null, int? sortOrder = null);

    /// <summary>
    ///     Moves an existing node to a new parent in the main navigation structure. If a
    ///     target parent key is provided, the node is moved under the specified parent.
    ///     If no target parent key is provided, the node is moved to the root level.
    /// </summary>
    /// <param name="key">The unique identifier of the node to move.</param>
    /// <param name="targetParentKey">
    ///     The unique identifier of the new parent node. If <c>null</c>, the node will be moved to
    ///     the root level.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully moved to the new parent
    ///     in the main navigation structure; otherwise, <c>false</c>.
    /// </returns>
    bool Move(Guid key, Guid? targetParentKey = null);

    /// <summary>
    ///     Updates the sort order of a node in the main navigation structure.
    ///     The sort order of other nodes in the same level will be adjusted accordingly.
    /// </summary>
    /// <param name="key">The unique identifier of the node to update.</param>
    /// <param name="newSortOrder">The new sort order for the node.</param>
    /// <returns>
    ///     <c>true</c> if the node's sort order was successfully updated; otherwise, <c>false</c>.
    /// </returns>
    bool UpdateSortOrder(Guid key, int newSortOrder);
}
