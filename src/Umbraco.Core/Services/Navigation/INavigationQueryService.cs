namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for querying the navigation structure.
/// </summary>
public interface INavigationQueryService
{
    bool TryGetParentKey(Guid childKey, out Guid? parentKey);

    bool TryGetRootKeys(out IEnumerable<Guid> rootKeys);

    bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys);

    bool TryGetLevel(Guid contentKey, out int level);
}
