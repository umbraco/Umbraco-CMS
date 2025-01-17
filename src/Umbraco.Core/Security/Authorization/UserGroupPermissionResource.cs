using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the <see cref="UserGroupPermissionHandler" />.
/// </summary>
public class UserGroupPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="UserGroupPermissionResource" /> with the specified key.
    /// </summary>
    /// <param name="userGroupKey">The key of the user group.</param>
    /// <returns>An instance of <see cref="UserGroupPermissionResource" />.</returns>
    public static UserGroupPermissionResource WithKeys(Guid userGroupKey) => WithKeys(userGroupKey.Yield());

    /// <summary>
    ///     Creates a <see cref="UserGroupPermissionResource" /> with the specified keys.
    /// </summary>
    /// <param name="userGroupKeys">The keys of the user groups.</param>
    /// <returns>An instance of <see cref="UserGroupPermissionResource" />.</returns>
    public static UserGroupPermissionResource WithKeys(IEnumerable<Guid> userGroupKeys) => new(userGroupKeys);

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserGroupPermissionResource" /> class.
    /// </summary>
    /// <param name="userGroupKeys">The keys of the user groups.</param>
    public UserGroupPermissionResource(IEnumerable<Guid> userGroupKeys) => UserGroupKeys = userGroupKeys;

    /// <summary>
    ///     Gets the user group keys.
    /// </summary>
    public IEnumerable<Guid> UserGroupKeys { get; }
}
