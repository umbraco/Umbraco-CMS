namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     A resource used for the <see cref="UserPermissionResource" />.
/// </summary>
public class UserPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPermissionResource" /> class.
    /// </summary>
    /// <param name="userKey">The key of the user item.</param>
    public UserPermissionResource(Guid userKey)
    {
        UserKeys = new List<Guid> { userKey };
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPermissionResource" /> class.
    /// </summary>
    /// <param name="userKeys">The keys of the user items.</param>
    public UserPermissionResource(IEnumerable<Guid> userKeys)
    {
        UserKeys = userKeys;
    }

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> UserKeys { get; }

}
