using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the <see cref="ElementContainerPermissionHandler" />.
/// </summary>
public class ElementContainerPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="ElementContainerPermissionResource" /> with the specified permission and container key or root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="containerKey">The key of the container or null if root.</param>
    /// <returns>An instance of <see cref="ElementContainerPermissionResource" />.</returns>
    public static ElementContainerPermissionResource WithKeys(string permissionToCheck, Guid? containerKey) =>
        containerKey is null
            ? Root(permissionToCheck)
            : WithKeys(permissionToCheck, containerKey.Value.Yield());

    /// <summary>
    ///     Creates a <see cref="ElementContainerPermissionResource" /> with the specified permission and container key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="containerKey">The key of the container.</param>
    /// <returns>An instance of <see cref="ElementContainerPermissionResource" />.</returns>
    public static ElementContainerPermissionResource WithKeys(string permissionToCheck, Guid containerKey) =>
        WithKeys(permissionToCheck, containerKey.Yield());

    /// <summary>
    ///     Creates a <see cref="ElementContainerPermissionResource" /> with the specified permission and container keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="containerKeys">The keys of the containers.</param>
    /// <returns>An instance of <see cref="ElementContainerPermissionResource" />.</returns>
    public static ElementContainerPermissionResource WithKeys(string permissionToCheck, IEnumerable<Guid> containerKeys) =>
        new(containerKeys, new HashSet<string> { permissionToCheck }, false, false);

    /// <summary>
    ///     Creates a <see cref="ElementContainerPermissionResource" /> with the specified permission and the root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ElementContainerPermissionResource" />.</returns>
    public static ElementContainerPermissionResource Root(string permissionToCheck) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, true, false);

    /// <summary>
    ///     Creates a <see cref="ElementContainerPermissionResource" /> with the specified permission and the recycle bin.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ElementContainerPermissionResource" />.</returns>
    public static ElementContainerPermissionResource RecycleBin(string permissionToCheck) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, false, true);

    private ElementContainerPermissionResource(
        IEnumerable<Guid> containerKeys,
        ISet<string> permissionsToCheck,
        bool checkRoot,
        bool checkRecycleBin)
    {
        ContainerKeys = containerKeys;
        PermissionsToCheck = permissionsToCheck;
        CheckRoot = checkRoot;
        CheckRecycleBin = checkRecycleBin;
    }

    /// <summary>
    ///     Gets the container keys.
    /// </summary>
    public IEnumerable<Guid> ContainerKeys { get; }

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
