namespace Umbraco.Cms.Core.Services.Navigation;

public interface INavigationService
{
    Task RebuildAsync();

    bool TryGetParentKey(Guid childKey, out Guid? parentKey);

    bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    IEnumerable<Guid> GetSiblingsKeys(Guid key);

    bool Remove(Guid key);

    bool Add(Guid key, Guid? parentKey = null);

    bool Copy(Guid sourceKey, out Guid copiedNodeKey, Guid? targetParentKey = null);

    bool Move(Guid nodeKey, Guid? targetParentKey = null);
}
