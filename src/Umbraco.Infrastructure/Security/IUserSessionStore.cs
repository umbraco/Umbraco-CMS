namespace Umbraco.Cms.Core.Security;

/// <summary>
///     An IUserStore interface part to implement if the store supports validating user session Ids
/// </summary>
/// <typeparam name="TUser">The user type</typeparam>
public interface IUserSessionStore<TUser>
    where TUser : class
{
    /// <summary>
    ///     Asynchronously determines whether the specified user's session is still valid.
    /// </summary>
    /// <param name="userId">The ID of the user whose session validity is being checked.</param>
    /// <param name="sessionId">The session ID to validate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is <c>true</c> if the session is valid; otherwise, <c>false</c>.</returns>
    Task<bool> ValidateSessionIdAsync(string? userId, string? sessionId);
}
