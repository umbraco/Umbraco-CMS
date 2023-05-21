namespace Umbraco.Cms.Core.Security;

/// <summary>
///     An IUserStore interface part to implement if the store supports validating user session Ids
/// </summary>
/// <typeparam name="TUser">The user type</typeparam>
public interface IUserSessionStore<TUser>
    where TUser : class
{
    /// <summary>
    ///     Validates a user's session is still valid
    /// </summary>
    Task<bool> ValidateSessionIdAsync(string? userId, string? sessionId);
}
