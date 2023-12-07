using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security.Authorization.UserGroup;

/// <summary>
///     A resource used for the <see cref="UserGroupPermissionResource" />.
/// </summary>
public class UserGroupPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a UserGroupPermissionResource with the specified Key.
    /// </summary>
    /// <param name="userGroupKey">The key of the user group.</param>
    /// <returns>An instance of UserGroupPermissionResource.</returns>
    public static UserGroupPermissionResource WithKeys(Guid userGroupKey) => WithKeys(userGroupKey.Yield());

    /// <summary>
    ///     Creates a UserGroupPermissionResource with the specified Keys.
    /// </summary>
    /// <param name="userGroupKeys">The keys of the user groups.</param>
    /// <returns>An instance of UserGroupPermissionResource.</returns>
    public static UserGroupPermissionResource WithKeys(IEnumerable<Guid> userGroupKeys) => new(userGroupKeys);

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionResource" /> class.
    /// </summary>
    /// <param name="userGroupKeys">The keys of the user group items.</param>
    public UserGroupPermissionResource(IEnumerable<Guid> userGroupKeys) => UserGroupKeys = userGroupKeys;

    /// <summary>
    ///     Gets the content keys.
    /// </summary>
    public IEnumerable<Guid> UserGroupKeys { get; }

}
