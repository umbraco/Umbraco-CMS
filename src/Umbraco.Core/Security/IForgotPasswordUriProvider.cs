using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to create forgot password URIs for users.
/// </summary>
public interface IForgotPasswordUriProvider
{
    /// <summary>
    ///     Creates a forgot password URI for the specified user.
    /// </summary>
    /// <param name="user">The user to create the forgot password URI for.</param>
    /// <returns>An attempt containing the generated URI or an error status.</returns>
    /// <remarks>
    ///     Failures that do not depend on the user should also be reported by
    ///     <see cref="CanCreateForgotPasswordUriAsync" />, so they are reported the same way whether or not the email
    ///     belongs to a user.
    /// </remarks>
    Task<Attempt<Uri, UserOperationStatus>> CreateForgotPasswordUriAsync(IUser user);

    /// <summary>
    ///     Determines whether forgot password URIs can currently be created, regardless of the user they are for.
    /// </summary>
    /// <returns>A successful attempt, or a failed attempt with the reason URIs cannot be created.</returns>
    /// <remarks>
    ///     This is checked before the user is looked up, so the outcome must not depend on any particular user.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    Task<Attempt<UserOperationStatus>> CanCreateForgotPasswordUriAsync()
        => Task.FromResult(Attempt.Succeed(UserOperationStatus.Success));
}
