namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <summary>
///     A resource used for the <see cref="UserGroupPermissionResource" />.
/// </summary>
public class UserGroupPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionResource" /> class.
    /// </summary>
    /// <param name="userGroupKey">The key of the user item.</param>
    public UserGroupPermissionResource(Guid userGroupKey)
    {
        UserGroupKeys = new List<Guid> { userGroupKey };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionResource" /> class.
    /// </summary>
    /// <param name="userKeys">The keys of the user items.</param>
    public UserGroupPermissionResource(IEnumerable<Guid> userGroupGroupKeys)
    {
        UserGroupKeys = userGroupGroupKeys;
    }

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> UserGroupKeys { get; }

}
