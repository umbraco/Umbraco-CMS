using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the <see cref="ElementPermissionHandler" />.
/// </summary>
public class ElementPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element key or root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKey">The key of the element or null if root.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(string permissionToCheck, Guid? elementKey) =>
        elementKey is null
            ? Root(permissionToCheck)
            : WithKeys(permissionToCheck, elementKey.Value.Yield());

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element key or root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKey">The key of the element or null if root.</param>
    /// <param name="cultures">The cultures to validate</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(
        string permissionToCheck,
        Guid? elementKey,
        IEnumerable<string> cultures) =>
        elementKey is null
            ? Root(permissionToCheck, cultures)
            : WithKeys(permissionToCheck, elementKey.Value.Yield(), cultures);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKeys">The keys of the elements or null if root.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(string permissionToCheck, IEnumerable<Guid?> elementKeys)
    {
        var hasRoot = elementKeys.Any(x => x is null);
        IEnumerable<Guid> keys = elementKeys.Where(x => x.HasValue).Select(x => x!.Value);

        return new ElementPermissionResource(keys, new HashSet<string> { permissionToCheck }, hasRoot, false, null, null);
    }

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKey">The key of the element.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(string permissionToCheck, Guid elementKey) =>
        WithKeys(permissionToCheck, elementKey.Yield());

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKey">The key of the element.</param>
    /// <param name="cultures">The required culture access</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(
        string permissionToCheck,
        Guid elementKey,
        IEnumerable<string> cultures) => WithKeys(permissionToCheck, elementKey.Yield(), cultures);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKeys">The keys of the elements.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(string permissionToCheck, IEnumerable<Guid> elementKeys) =>
        new(elementKeys, new HashSet<string> { permissionToCheck }, false, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and element keys.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="elementKeys">The keys of the elements.</param>
    /// <param name="cultures">The required culture access</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(
        string permissionToCheck,
        IEnumerable<Guid> elementKeys,
        IEnumerable<string> cultures) =>
        new(
            elementKeys,
            new HashSet<string> { permissionToCheck },
            false,
            false,
            null,
            new HashSet<string>(cultures.Distinct()));

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permissions and element keys.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <param name="elementKeys">The keys of the elements.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource WithKeys(ISet<string> permissionsToCheck, IEnumerable<Guid> elementKeys) =>
        new(elementKeys, permissionsToCheck, false, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and the root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Root(string permissionToCheck) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, true, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and the root.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="cultures">The cultures to validate</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Root(string permissionToCheck, IEnumerable<string> cultures) =>
        new(
            Enumerable.Empty<Guid>(),
            new HashSet<string> { permissionToCheck },
            true,
            false,
            null,
            new HashSet<string>(cultures));

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permissions and the root.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Root(ISet<string> permissionsToCheck) =>
        new(Enumerable.Empty<Guid>(), permissionsToCheck, true, false, null, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permissions and the root.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <param name="cultures">The cultures to validate</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Root(ISet<string> permissionsToCheck, IEnumerable<string> cultures) =>
        new(Enumerable.Empty<Guid>(), permissionsToCheck, true, false, null, new HashSet<string>(cultures));


    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permissions and the recycle bin.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource RecycleBin(ISet<string> permissionsToCheck) =>
        new(Enumerable.Empty<Guid>(), permissionsToCheck, false, true, null, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and the recycle bin.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource RecycleBin(string permissionToCheck) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, false, true, null, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permissions and the branch from the specified parent key.
    /// </summary>
    /// <param name="permissionsToCheck">The permissions to check for.</param>
    /// <param name="parentKeyForBranch">The parent key of the branch.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Branch(ISet<string> permissionsToCheck, Guid parentKeyForBranch) =>
        new(Enumerable.Empty<Guid>(), permissionsToCheck, false, true, parentKeyForBranch, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and the branch from the specified parent key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="parentKeyForBranch">The parent key of the branch.</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Branch(string permissionToCheck, Guid parentKeyForBranch) =>
        new(Enumerable.Empty<Guid>(), new HashSet<string> { permissionToCheck }, false, true, parentKeyForBranch, null);

    /// <summary>
    ///     Creates a <see cref="ElementPermissionResource" /> with the specified permission and the branch from the specified parent key.
    /// </summary>
    /// <param name="permissionToCheck">The permission to check for.</param>
    /// <param name="parentKeyForBranch">The parent key of the branch.</param>
    /// <param name="culturesToCheck">The required cultures</param>
    /// <returns>An instance of <see cref="ElementPermissionResource" />.</returns>
    public static ElementPermissionResource Branch(
        string permissionToCheck,
        Guid parentKeyForBranch,
        IEnumerable<string> culturesToCheck) =>
        new(
            Enumerable.Empty<Guid>(),
            new HashSet<string> { permissionToCheck },
            false,
            true,
            parentKeyForBranch,
            new HashSet<string>(culturesToCheck.Distinct()));

    private ElementPermissionResource(
        IEnumerable<Guid> elementKeys,
        ISet<string> permissionsToCheck,
        bool checkRoot,
        bool checkRecycleBin,
        Guid? parentKeyForBranch,
        ISet<string>? culturesToCheck)
    {
        ElementKeys = elementKeys;
        PermissionsToCheck = permissionsToCheck;
        CheckRoot = checkRoot;
        CheckRecycleBin = checkRecycleBin;
        ParentKeyForBranch = parentKeyForBranch;
        CulturesToCheck = culturesToCheck;
    }

    /// <summary>
    ///     Gets the element keys.
    /// </summary>
    public IEnumerable<Guid> ElementKeys { get; }

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

    /// <summary>
    ///     Gets the parent key of a branch.
    /// </summary>
    public Guid? ParentKeyForBranch { get; }

    /// <summary>
    /// All the cultures need to be accessible when evaluating
    /// </summary>
    public ISet<string>? CulturesToCheck { get; }
}