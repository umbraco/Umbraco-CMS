using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the <see cref="ElementFolderPermissionHandler" />.
/// </summary>
public class ElementFolderPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="ElementFolderPermissionResource" /> with the specified permission and folder key or root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="folderKey">The key of the folder or null if root.</param>
    /// <returns>An instance of <see cref="ElementFolderPermissionResource" />.</returns>
    public static ElementFolderPermissionResource WithKeys(string permissionToCheck, Guid? folderKey) =>
        folderKey is null
            ? Root(permissionToCheck)
            : WithKeys(permissionToCheck, folderKey.Value.Yield());

    /// <summary>
    ///     Creates a <see cref="ElementFolderPermissionResource" /> with the specified permission and folder key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="folderKey">The key of the folder.</param>
    /// <returns>An instance of <see cref="ElementFolderPermissionResource" />.</returns>
    public static ElementFolderPermissionResource WithKeys(string permissionToCheck, Guid folderKey) =>
        WithKeys(permissionToCheck, folderKey.Yield());

    /// <summary>
    ///     Creates a <see cref="ElementFolderPermissionResource" /> with the specified permission and folder keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="folderKeys">The keys of the folders.</param>
    /// <returns>An instance of <see cref="ElementFolderPermissionResource" />.</returns>
    public static ElementFolderPermissionResource WithKeys(string permissionToCheck, IEnumerable<Guid> folderKeys) =>
        new(folderKeys, new HashSet<string> { permissionToCheck }, false, false);

    /// <summary>
    ///     Creates a <see cref="ElementFolderPermissionResource" /> with the specified permission and the root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ElementFolderPermissionResource" />.</returns>
    public static ElementFolderPermissionResource Root(string permissionToCheck) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, true, false);

    /// <summary>
    ///     Creates a <see cref="ElementFolderPermissionResource" /> with the specified permission and the recycle bin.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ElementFolderPermissionResource" />.</returns>
    public static ElementFolderPermissionResource RecycleBin(string permissionToCheck) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, false, true);

    private ElementFolderPermissionResource(
        IEnumerable<Guid> folderKeys,
        ISet<string> permissionsToCheck,
        bool checkRoot,
        bool checkRecycleBin)
    {
        FolderKeys = folderKeys;
        PermissionsToCheck = permissionsToCheck;
        CheckRoot = checkRoot;
        CheckRecycleBin = checkRecycleBin;
    }

    /// <summary>
    ///     Gets the folder keys.
    /// </summary>
    public IEnumerable<Guid> FolderKeys { get; }

    /// <summary>
    ///     Gets the collection of permissions to authorize.
    /// </summary>
    /// <remarks>
    ///     All permissions have to be satisfied when evaluating.
    /// </remarks>
    public ISet<string> PermissionsToCheck { get; }

    /// <summary>
    ///     Gets a value indicating whether to check for the root.
    /// </summary>
    public bool CheckRoot { get; }

    /// <summary>
    ///     Gets a value indicating whether to check for the recycle bin.
    /// </summary>
    public bool CheckRecycleBin { get; }
}
