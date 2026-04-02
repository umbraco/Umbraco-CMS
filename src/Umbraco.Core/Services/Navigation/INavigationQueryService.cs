using System.Diagnostics.CodeAnalysis;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for querying the navigation structure.
/// </summary>
/// <remarks>
///     This interface defines methods for traversing and querying the main navigation structure,
///     including operations for retrieving parent, children, descendants, ancestors, siblings,
///     and level information for content nodes.
/// </remarks>
public interface INavigationQueryService
{
    /// <summary>
    ///     Attempts to get the parent key of a child node.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="parentKey">
    ///     When this method returns, contains the parent's unique identifier if the child exists;
    ///     otherwise, <c>null</c>. The value will be <c>null</c> if the child is at root level.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetParentKey(Guid childKey, out Guid? parentKey);

    /// <summary>
    ///     Attempts to get all root-level node keys.
    /// </summary>
    /// <param name="rootKeys">
    ///     When this method returns, contains the collection of root node keys ordered by sort order.
    /// </param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    bool TryGetRootKeys(out IEnumerable<Guid> rootKeys);

    /// <summary>
    ///     Attempts to get all root-level node keys of a specific content type.
    /// </summary>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="rootKeys">
    ///     When this method returns, contains the collection of root node keys of the specified
    ///     content type, ordered by sort order; or an empty collection if the content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the content type exists and root keys were retrieved; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetRootKeysOfType(string contentTypeAlias, out IEnumerable<Guid> rootKeys);

    /// <summary>
    ///     Attempts to get all child node keys of a parent node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="childrenKeys">
    ///     When this method returns, contains the collection of child node keys ordered by sort order;
    ///     or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    /// <summary>
    ///     Attempts to get all child node keys of a specific content type under a parent node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="childrenKeys">
    ///     When this method returns, contains the collection of child node keys of the specified
    ///     content type, ordered by sort order; or an empty collection if the parent or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the parent and content type exist and children were retrieved; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetChildrenKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> childrenKeys);

    /// <summary>
    ///     Attempts to get all descendant node keys of a parent node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="descendantsKeys">
    ///     When this method returns, contains the collection of all descendant node keys
    ///     (children, grandchildren, etc.); or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    /// <summary>
    ///     Attempts to get all descendant node keys including the parent node itself.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="descendantsOrSelfKeys">
    ///     When this method returns, contains the parent's key followed by all its descendant keys;
    ///     or an empty collection if the parent doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the parent node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetDescendantsKeysOrSelfKeys(Guid parentKey, out IEnumerable<Guid> descendantsOrSelfKeys)
    {
        if (TryGetDescendantsKeys(parentKey, out IEnumerable<Guid>? descendantsKeys))
        {
            descendantsOrSelfKeys = parentKey.Yield().Concat(descendantsKeys);
            return true;
        }

        descendantsOrSelfKeys = Array.Empty<Guid>();
        return false;
    }

    /// <summary>
    ///     Attempts to get all descendant node keys of a specific content type under a parent node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the parent node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="descendantsKeys">
    ///     When this method returns, contains the collection of descendant node keys of the specified
    ///     content type; or an empty collection if the parent or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the parent and content type exist and descendants were retrieved; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetDescendantsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> descendantsKeys);

    /// <summary>
    ///     Attempts to get all ancestor node keys of a child node.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="ancestorsKeys">
    ///     When this method returns, contains the collection of ancestor node keys
    ///     (parent, grandparent, etc.); or an empty collection if the child doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    /// <summary>
    ///     Attempts to get all ancestor node keys including the child node itself.
    /// </summary>
    /// <param name="childKey">The unique identifier of the child node.</param>
    /// <param name="ancestorsOrSelfKeys">
    ///     When this method returns, contains the child's key followed by all its ancestor keys;
    ///     or an empty collection if the child doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the child node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetAncestorsOrSelfKeys(Guid childKey, out IEnumerable<Guid> ancestorsOrSelfKeys)
    {
        if (TryGetAncestorsKeys(childKey, out IEnumerable<Guid>? ancestorsKeys))
        {
            ancestorsOrSelfKeys = childKey.Yield().Concat(ancestorsKeys);
            return true;
        }

        ancestorsOrSelfKeys = Array.Empty<Guid>();
        return false;
    }

    /// <summary>
    ///     Attempts to get all ancestor node keys of a specific content type for a given node.
    /// </summary>
    /// <param name="parentKey">The unique identifier of the node to find ancestors for.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="ancestorsKeys">
    ///     When this method returns, contains the collection of ancestor node keys of the specified
    ///     content type; or an empty collection if the node or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and content type exist and ancestors were retrieved; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetAncestorsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> ancestorsKeys);

    /// <summary>
    ///     Attempts to get all sibling node keys of a node.
    /// </summary>
    /// <param name="key">The unique identifier of the node.</param>
    /// <param name="siblingsKeys">
    ///     When this method returns, contains the collection of sibling node keys
    ///     (excluding the node itself), ordered by sort order; or an empty collection if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists in the structure; otherwise, <c>false</c>.</returns>
    bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys);

    /// <summary>
    ///     Attempts to get all sibling node keys of a specific content type for a given node.
    /// </summary>
    /// <param name="key">The unique identifier of the node.</param>
    /// <param name="contentTypeAlias">The alias of the content type to filter by.</param>
    /// <param name="siblingsKeys">
    ///     When this method returns, contains the collection of sibling node keys of the specified
    ///     content type, ordered by sort order; or an empty collection if the node or content type doesn't exist.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the node and content type exist and siblings were retrieved; otherwise, <c>false</c>.
    /// </returns>
    bool TryGetSiblingsKeysOfType(Guid key, string contentTypeAlias, out IEnumerable<Guid> siblingsKeys);

    /// <summary>
    ///     Attempts to get the hierarchical level of a content node.
    /// </summary>
    /// <param name="contentKey">The unique identifier of the content node.</param>
    /// <param name="level">
    ///     When this method returns, contains the level of the node (1 for root-level nodes,
    ///     2 for their children, etc.); or <c>null</c> if the node doesn't exist.
    /// </param>
    /// <returns><c>true</c> if the node exists and its level was determined; otherwise, <c>false</c>.</returns>
    bool TryGetLevel(Guid contentKey, [NotNullWhen(true)] out int? level);
}
