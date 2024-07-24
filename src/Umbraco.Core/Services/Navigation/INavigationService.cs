namespace Umbraco.Cms.Core.Services.Navigation;

public interface INavigationService
{
    Task<Guid?> GetParentKeyAsync(Guid childKey);

    Task<IEnumerable<Guid>> GetChildrenKeysAsync(Guid parentKey);

    Task<IEnumerable<Guid>> GetDescendantsKeysAsync(Guid parentKey);

    Task<IEnumerable<Guid>> GetAncestorsKeysAsync(Guid childKey);

    Task<IEnumerable<Guid>> GetSiblingsKeysAsync(Guid key);

    Task RebuildAsync();

    bool Remove(Guid key);

    bool Add(Guid key, Guid? parentKey = null);

    bool Copy(Guid sourceKey, out Guid copiedNodeKey, Guid? targetParentKey = null);

    bool Move(Guid nodeKey, Guid? targetParentKey = null);
}
