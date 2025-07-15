using System.Diagnostics.CodeAnalysis;
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

    bool TryGetRootKeysOfType(string contentTypeAlias, out IEnumerable<Guid> rootKeys);

    bool TryGetChildrenKeys(Guid parentKey, out IEnumerable<Guid> childrenKeys);

    bool TryGetChildrenKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> childrenKeys);

    bool TryGetDescendantsKeys(Guid parentKey, out IEnumerable<Guid> descendantsKeys);

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

    bool TryGetDescendantsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> descendantsKeys);

    bool TryGetAncestorsKeys(Guid childKey, out IEnumerable<Guid> ancestorsKeys);

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

    bool TryGetAncestorsKeysOfType(Guid parentKey, string contentTypeAlias, out IEnumerable<Guid> ancestorsKeys);

    bool TryGetSiblingsKeys(Guid key, out IEnumerable<Guid> siblingsKeys);

    bool TryGetSiblingsKeysOfType(Guid key, string contentTypeAlias, out IEnumerable<Guid> siblingsKeys);

    bool TryGetLevel(Guid contentKey, [NotNullWhen(true)] out int? level);
}
