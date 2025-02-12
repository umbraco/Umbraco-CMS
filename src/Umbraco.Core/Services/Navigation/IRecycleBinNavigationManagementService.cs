namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for managing the recycle bin navigation structure.
/// </summary>
public interface IRecycleBinNavigationManagementService
{
    /// <summary>
    ///     Rebuilds the recycle bin navigation structure by fetching the latest trashed nodes
    ///     from the underlying repository.
    /// </summary>
    Task RebuildBinAsync();

    /// <summary>
    ///     Permanently removes a node and all of its descendants from the recycle bin navigation structure.
    /// </summary>
    /// <param name="key">The unique identifier of the node to remove.</param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully removed from the recycle bin;
    ///     otherwise, <c>false</c>.
    /// </returns>
    bool RemoveFromBin(Guid key);

    /// <summary>
    ///     Restores a node and all of its descendants from the recycle bin navigation structure and moves them back
    ///     to the main navigation structure. The node can be restored to a specified target parent or to the root
    ///     level if no parent is specified.
    /// </summary>
    /// <param name="key">The unique identifier of the node to restore from the recycle bin navigation structure.</param>
    /// <param name="targetParentKey">
    ///     The unique identifier of the target parent node in the main navigation structure to which the node
    ///     should be restored. If <c>null</c>, the node will be restored to the root level.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and its descendants were successfully restored to the main navigation structure;
    ///     otherwise, <c>false</c>.
    /// </returns>
    bool RestoreFromBin(Guid key, Guid? targetParentKey = null);
}
