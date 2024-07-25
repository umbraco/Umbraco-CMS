namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Manages navigation-related operations.
/// </summary>
public interface INavigationService
{
    Task RebuildAsync();

    bool TryGetParentKey(Guid childKey, out Guid? parentKey);

    bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys);

    bool Remove(Guid key);

    bool Add(Guid key, Guid? parentKey = null);

    bool Move(Guid nodeKey, Guid? targetParentKey = null);
}
