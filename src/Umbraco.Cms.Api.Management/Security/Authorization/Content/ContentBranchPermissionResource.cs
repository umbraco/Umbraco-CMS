namespace Umbraco.Cms.Api.Management.Security.Authorization.Content;

/// <summary>
///     A resource used for the <see cref="ContentPermissionRequirement" />.
/// </summary>
public class ContentBranchPermissionResource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentBranchPermissionResource" /> class.
    /// </summary>
    /// <param name="parentKey">The key of the parent content item.</param>
    /// <param name="permissionToCheck">The permission to authorize.</param>
    public ContentBranchPermissionResource(Guid parentKey, char permissionToCheck)
    {
        ParentKey = parentKey;
        PermissionsToCheck = new List<char> { permissionToCheck };
    }

    /// <summary>
    ///     Gets the parent key.
    /// </summary>
    public Guid ParentKey { get; }

    /// <summary>
    ///     Gets the collection of permissions to authorize.
    /// </summary>
    /// <remarks>
    ///     All permissions have to be satisfied when evaluating.
    /// </remarks>
    public IReadOnlyList<char> PermissionsToCheck { get; }
}
