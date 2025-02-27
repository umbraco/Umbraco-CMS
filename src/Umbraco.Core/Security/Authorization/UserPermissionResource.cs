using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     A resource used for the <see cref="UserPermissionHandler" />.
/// </summary>
public class UserPermissionResource : IPermissionResource
{
    /// <summary>
    ///     Creates a <see cref="UserPermissionResource" /> with the specified keys.
    /// </summary>
    /// <param name="userKey">The key of the user.</param>
    /// <returns>An instance of <see cref="UserPermissionResource" />.</returns>
    public static UserPermissionResource WithKeys(Guid userKey) => WithKeys(userKey.Yield());

    /// <summary>
    ///     Creates a <see cref="UserPermissionResource" /> with the specified keys.
    /// </summary>
    /// <param name="userKeys">The keys of the users.</param>
    /// <returns>An instance of <see cref="UserPermissionResource" />.</returns>
    public static UserPermissionResource WithKeys(IEnumerable<Guid> userKeys) => new(userKeys);

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserPermissionResource" /> class.
    /// </summary>
    /// <param name="userKeys">The keys of the users.</param>
    private UserPermissionResource(IEnumerable<Guid> userKeys) => UserKeys = userKeys;

    /// <summary>
    ///     Gets the user keys.
    /// </summary>
    public IEnumerable<Guid> UserKeys { get; }
}
