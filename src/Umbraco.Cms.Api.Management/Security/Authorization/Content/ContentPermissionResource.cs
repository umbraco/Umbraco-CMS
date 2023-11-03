using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     The resource used for the <see cref="ContentPermissionRequirement" />.
/// </summary>
public class ContentPermissionResource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionResource" /> class.
    /// </summary>
    /// <param name="contentKey">The key of the content item.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    public ContentPermissionResource(Guid? contentKey, char permissionToCheck)
    {
        ContentKeys = new List<Guid?> { contentKey };
        PermissionsToCheck = new List<char> { permissionToCheck };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPermissionResource" /> class.
    /// </summary>
    /// <param name="contentKeys">The keys of the content items.</param>
    /// <param name="permissionsToCheck">The collection of permissions to authorize.</param>
    public ContentPermissionResource(IEnumerable<Guid?> contentKeys, IReadOnlyList<char> permissionsToCheck)
    {
        ContentKeys = contentKeys;
        PermissionsToCheck = permissionsToCheck;
    }

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    /// <remarks>
    ///     When content key is null, it is an indicator for the global system root node <see cref="Constants.System.RootKey" />.
    /// </remarks>
    public IEnumerable<Guid?> ContentKeys { get; }

    /// <summary>
    ///     Gets the collection of permissions to authorize.
    /// </summary>
    /// <remarks>
    ///     All permissions have to be satisfied when evaluating.
    /// </remarks>
    public IReadOnlyList<char> PermissionsToCheck { get; }
}
