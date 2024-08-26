namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
///     Placeholder for sharing logic between the document and media navigation services
///     for querying the recycle bin navigation structure.
/// </summary>
public interface IRecycleBinNavigationQueryService
{
    bool TryGetParentKeyInBin(Guid childKey, out Guid? parentKey);

    bool TryGetChildrenKeysInBin(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    bool TryGetDescendantsKeysInBin(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    bool TryGetAncestorsKeysInBin(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    bool TryGetSiblingsKeysInBin(Guid key, out IEnumerable<Guid> siblingsKeys);
}
