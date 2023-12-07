using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.User;

/// <summary>
///     A resource used for the <see cref="UserPermissionResource" />.
/// </summary>
public class UserPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a UserPermissionResource with the specified Keys.
    /// </summary>
    /// <param name="userKey">The key of the user.</param>
    /// <returns>An instance of UserPermissionResource.</returns>
    public static UserPermissionResource WithKeys(Guid userKey) => WithKeys(userKey.Yield());

    /// <summary>
    ///     Creates a UserPermissionResource with the specified Keys.
    /// </summary>
    /// <param name="userKeys">The keys of the users.</param>
    /// <returns>An instance of UserPermissionResource.</returns>
    public static UserPermissionResource WithKeys(IEnumerable<Guid> userKeys) => new(userKeys);

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPermissionResource" /> class.
    /// </summary>
    /// <param name="userKeys">The keys of the user items.</param>
    private UserPermissionResource(IEnumerable<Guid> userKeys) => UserKeys = userKeys;

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> UserKeys { get; }

}
