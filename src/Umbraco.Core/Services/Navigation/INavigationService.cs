namespace Umbraco.Cms.Core.Services.Navigation;

public interface INavigationService
{
    Task RebuildAsync();

    Guid? GetParentKey(Guid childKey);

    IEnumerable<Guid> GetChildrenKeys(Guid parentKey);

    IEnumerable<Guid> GetDescendantsKeys(Guid parentKey);

    IEnumerable<Guid> GetAncestorsKeys(Guid childKey);

    IEnumerable<Guid> GetSiblingsKeys(Guid key);

    bool Remove(Guid key);

    bool Add(Guid key, Guid? parentKey = null);

    bool Copy(Guid sourceKey, out Guid copiedNodeKey, Guid? targetParentKey = null);

    bool Move(Guid nodeKey, Guid? targetParentKey = null);
}
