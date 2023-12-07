using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     A resource used for the <see cref="ContentPermissionRequirement" />.
/// </summary>
public class ContentPermissionResource : IPermissionResource
{

    public static ContentPermissionResource WithKeys(char permissionToCheck, Guid? contentKey)
    {
        return contentKey is null
            ? Root(permissionToCheck)
            : WithKeys(permissionToCheck, contentKey.Value.Yield());
    }

    public static ContentPermissionResource WithKeys(char permissionToCheck, Guid contentKey) => WithKeys(permissionToCheck, contentKey.Yield());

    public static ContentPermissionResource WithKeys(char permissionToCheck, IEnumerable<Guid> contentKeys) =>
        new ContentPermissionResource(contentKeys, new HashSet<char>(){permissionToCheck}, false, false, null);
    public static ContentPermissionResource WithKeys(ISet<char> permissionsToCheck, IEnumerable<Guid> contentKeys) =>
        new ContentPermissionResource(contentKeys, permissionsToCheck, false, false, null);

    public static ContentPermissionResource Root(char permissionToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char>(){permissionToCheck}, true, false, null);

    public static ContentPermissionResource Root(ISet<char> permissionsToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck, true, false, null);

    public static ContentPermissionResource RecycleBin(ISet<char> permissionsToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck,  false, true, null);
    public static ContentPermissionResource RecycleBin(char permissionToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char>(){permissionToCheck},  false, true, null);

    public static ContentPermissionResource Branch(ISet<char> permissionsToCheck, Guid parentId) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck,  false, true, parentId);
    public static ContentPermissionResource Branch(char permissionToCheck, Guid parentId) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char>(){permissionToCheck},  false, true, parentId);




    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionResource" /> class.
    /// </summary>
    /// <param name="contentKeys">The keys of the content items.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    private ContentPermissionResource(IEnumerable<Guid> contentKeys, ISet<char> permissionsToCheck, bool checkRoot, bool checkRecyleBin,  Guid? parentKey)
    {
        ContentKeys = contentKeys;
        PermissionsToCheck = permissionsToCheck;
        CheckRoot = checkRoot;
        CheckRecyleBin = checkRecyleBin;
        ParentKey = parentKey;
    }

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> ContentKeys { get; }

    /// <summary>
    ///     Gets the collection of permissions to authorize.
    /// </summary>
    /// <remarks>
    ///     All permissions have to be satisfied when evaluating.
    /// </remarks>
    public ISet<char> PermissionsToCheck { get; }

    public bool CheckRoot { get; }
    public bool CheckRecyleBin { get; }
    public Guid? ParentKey { get; }
}
