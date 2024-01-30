using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     A resource used for the <see cref="ContentPermissionHandler" />.
/// </summary>
public class ContentPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content key or root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKey">The key of the content or null if root.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, Guid? contentKey) =>
        contentKey is null
            ? Root(permissionToCheck)
            : WithKeys(permissionToCheck, contentKey.Value.Yield());

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content key or root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKey">The key of the content or null if root.</param>
    /// <param name="cultures">The cultures to validate</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, Guid? contentKey, IEnumerable<string> cultures) =>
        contentKey is null
            ? Root(permissionToCheck, cultures)
            : WithKeys(permissionToCheck, contentKey.Value.Yield(), cultures);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKeys">The keys of the contents or null if root.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, IEnumerable<Guid?> contentKeys)
    {
        var hasRoot = contentKeys.Any(x => x is null);
        IEnumerable<Guid> keys = contentKeys.Where(x => x.HasValue).Select(x => x!.Value);

        return new ContentPermissionResource(keys, new HashSet<char> { permissionToCheck }, hasRoot, false, null, null);
    }

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKey">The key of the content.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, Guid contentKey) => WithKeys(permissionToCheck, contentKey.Yield());

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKey">The key of the content.</param>
    /// <param name="cultures">The required culture access</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, Guid contentKey,IEnumerable<string> cultures) => WithKeys(permissionToCheck, contentKey.Yield(),cultures);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKeys">The keys of the contents.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, IEnumerable<Guid> contentKeys) =>
        new ContentPermissionResource(contentKeys, new HashSet<char> { permissionToCheck }, false, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and content keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="contentKeys">The keys of the contents.</param>
    /// <param name="cultures">The required culture access</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(char permissionToCheck, IEnumerable<Guid> contentKeys, IEnumerable<string> cultures) =>
        new ContentPermissionResource(
            contentKeys,
            new HashSet<char> { permissionToCheck },
            false,
            false,
            null,
            new HashSet<string>(cultures.Distinct()));

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permissions and content keys.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <param name="contentKeys">The keys of the contents.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource WithKeys(ISet<char> permissionsToCheck, IEnumerable<Guid> contentKeys) =>
        new ContentPermissionResource(contentKeys, permissionsToCheck, false, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and the root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Root(char permissionToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char> { permissionToCheck }, true, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and the root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="cultures">The cultures to validate</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Root(char permissionToCheck, IEnumerable<string> cultures) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char> { permissionToCheck }, true, false, null, new HashSet<string>(cultures));

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permissions and the root.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Root(ISet<char> permissionsToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck, true, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permissions and the root.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <param name="cultures">The cultures to validate</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Root(ISet<char> permissionsToCheck, IEnumerable<string> cultures) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck, true, false, null, new HashSet<string>(cultures));



    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permissions and the recycle bin.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource RecycleBin(ISet<char> permissionsToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck,  false, true, null, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and the recycle bin.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource RecycleBin(char permissionToCheck) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char> { permissionToCheck },  false, true, null, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permissions and the branch from the specified parent key.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <param name="parentKeyForBranch">The parent key of the branch.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Branch(ISet<char> permissionsToCheck, Guid parentKeyForBranch) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), permissionsToCheck,  false, true, parentKeyForBranch, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and the branch from the specified parent key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="parentKeyForBranch">The parent key of the branch.</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Branch(char permissionToCheck, Guid parentKeyForBranch) =>
        new ContentPermissionResource(Enumerable.Empty<Guid>(), new HashSet<char> { permissionToCheck },  false, true, parentKeyForBranch, null);

    /// <summary>
    ///     Creates a <see cref="ContentPermissionResource" /> with the specified permission and the branch from the specified parent key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="parentKeyForBranch">The parent key of the branch.</param>
    /// <param name="culturesToCheck">The required cultures</param>
    /// <returns>An instance of <see cref="ContentPermissionResource" />.</returns>
    public static ContentPermissionResource Branch(char permissionToCheck, Guid parentKeyForBranch, IEnumerable<string> culturesToCheck) =>
        new ContentPermissionResource(
            Enumerable.Empty<Guid>(),
            new HashSet<char> { permissionToCheck },
            false,
            true,
            parentKeyForBranch,
            new HashSet<string>(culturesToCheck.Distinct()));

    private ContentPermissionResource(
        IEnumerable<Guid> contentKeys,
        ISet<char> permissionsToCheck,
        bool checkRoot, bool checkRecycleBin,
        Guid? parentKeyForBranch,
        ISet<string>? culturesToCheck)
    {
        ContentKeys = contentKeys;
        PermissionsToCheck = permissionsToCheck;
        CheckRoot = checkRoot;
        CheckRecycleBin = checkRecycleBin;
        ParentKeyForBranch = parentKeyForBranch;
        CulturesToCheck = culturesToCheck;
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

    /// <summary>
    ///     Gets a value indicating whether to check for the root.
    /// </summary>
    public bool CheckRoot { get; }

    /// <summary>
    ///     Gets a value indicating whether to check for the recycle bin.
    /// </summary>
    public bool CheckRecycleBin { get; }

    /// <summary>
    ///     Gets the parent key of a branch.
    /// </summary>
    public Guid? ParentKeyForBranch { get; }

    /// <summary>
    /// All the cultures need to be accessible when evaluating
    /// </summary>
    public ISet<string>? CulturesToCheck { get; }
}
