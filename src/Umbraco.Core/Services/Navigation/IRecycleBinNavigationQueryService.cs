using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for querying the recycle bin navigation structure.
/// </summary>
/// <remarks>
///     This interface defines methods for traversing and querying the recycle bin
///     navigation structure, including parent, children, descendants, ancestors, and siblings.
/// </remarks>
public interface IRecycleBinNavigationQueryService
{
    /// <summary>
    ///     Attempts to get the parent key of a child node in the recycle bin.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="parentKey">
    ///     When this method returns, contains the parent's unique identifier if the child exists;
    ///     otherwise, <c>null</c>. The value will be <c>null</c> if the child is at bin root level.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    bool TryGetParentKeyInBin(Guid childKey, out Guid? parentKey);

    /// <summary>
    ///     Attempts to get all child node keys of a parent node in the recycle bin.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="childrenKeys">
    ///     When this method returns, contains the collection of child node keys ordered by sort order;
    ///     or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    bool TryGetChildrenKeysInBin(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    /// <summary>
    ///     Attempts to get all descendant node keys of a parent node in the recycle bin.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="descendantsKeys">
    ///     When this method returns, contains the collection of all descendant node keys;
    ///     or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    bool TryGetDescendantsKeysInBin(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    /// <summary>
    ///     Attempts to get all descendant node keys including the node itself in the recycle bin.
    /// </summary>
    /// <param name="childKey">The unique identifier of the node.</param>
    /// <param name="descendantsOrSelfKeys">
    ///     When this method returns, contains the node's key followed by all its descendant keys;
    ///     or an empty collection if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    bool TryGetDescendantsKeysOrSelfKeysInBin(Guid childKey, out IEnumerable<Guid> descendantsOrSelfKeys)
    {
        if (TryGetDescendantsKeysInBin(childKey, out IEnumerable<Guid>? descendantsKeys))
        {
            descendantsOrSelfKeys = childKey.Yield().Concat(descendantsKeys);
            return true;
        }

        descendantsOrSelfKeys = Array.Empty<Guid>();
        return false;
    }


    /// <summary>
    ///     Attempts to get all ancestor node keys of a child node in the recycle bin.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="ancestorsKeys">
    ///     When this method returns, contains the collection of ancestor node keys
    ///     (parent, grandparent, etc.); or an empty collection if the child doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    bool TryGetAncestorsKeysInBin(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    /// <summary>
    ///     Attempts to get all sibling node keys of a node in the recycle bin.
    /// </summary>
    /// <param name="key">The unique identifier of the node.</param>
    /// <param name="siblingsKeys">
    ///     When this method returns, contains the collection of sibling node keys
    ///     (excluding the node itself), ordered by sort order; or an empty collection if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists in the recycle bin; otherwise, <c>false</c>.</returns>
    bool TryGetSiblingsKeysInBin(Guid key, out IEnumerable<Guid> siblingsKeys);
}
