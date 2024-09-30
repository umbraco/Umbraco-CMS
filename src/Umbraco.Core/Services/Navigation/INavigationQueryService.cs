using Umbraco.Extensions;

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
    bool TryGetRootKeys(out IEnumerable<Guid> childrenKeys);

    bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

    bool TryGetDescendantsKeysOrSelfKeys(Guid childKey, out IEnumerable<Guid> descendantsOrSelfKeys)
    {
        if(TryGetDescendantsKeys(childKey, out var descendantsKeys))
        {
            descendantsOrSelfKeys = childKey.Yield().Concat(descendantsKeys);
            return true;
        }

        descendantsOrSelfKeys = Array.Empty<Guid>();
        return false;
    }


    bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

    bool TryGetAncestorsOrSelfKeys(Guid childKey, out IEnumerable<Guid> ancestorsOrSelfKeys)
    {
        if(TryGetAncestorsKeys(childKey, out var ancestorsKeys))
        {
            ancestorsOrSelfKeys =  childKey.Yield().Concat(ancestorsKeys);
            return true;
        }

        ancestorsOrSelfKeys = Array.Empty<Guid>();
        return false;
    }

    bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys);
}
