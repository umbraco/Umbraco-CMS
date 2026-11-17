using System.Security.Principal;
using Umbraco.Cms.Infrastructure.Security;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     The user manager for the back office
/// </summary>
public interface IBackOfficeUserManager : IUmbracoUserManager<BackOfficeIdentityUser>
{
    /// <summary>
    /// Notifies the system that a forgot password request has been made for a back office user.
    /// </summary>
    /// <param name="currentUser">The principal representing the user initiating the notification, typically an administrator or system process.</param>
    /// <param name="userId">The unique identifier of the user for whom the password reset was requested.</param>
    [Obsolete("Use NotifyForgotPasswordRequestedAsync instead. Scheduled for removal in Umbraco 21.")]
    void NotifyForgotPasswordRequested(IPrincipal currentUser, string userId);

    /// <summary>
    /// Notifies the system that a user's password has been changed as a result of completing the forgot password process.
    /// </summary>
    /// <param name="currentUser">The principal representing the user performing the notification action.</param>
    /// <param name="userId">The unique identifier of the user whose password was changed.</param>
    [Obsolete("Use NotifyForgotPasswordChangedAsync instead. Scheduled for removal in Umbraco 21.")]
    void NotifyForgotPasswordChanged(IPrincipal currentUser, string userId);

    /// <summary>
    /// Notifies the system that a logout operation has succeeded for a specified user.
    /// </summary>
    /// <param name="currentUser">The principal representing the user performing the logout.</param>
    /// <param name="userId">The optional identifier of the user who logged out; may be <c>null</c>.</param>
    /// <returns>A <see cref="SignOutSuccessResult"/> representing the outcome of the logout notification.</returns>
    [Obsolete("Use NotifyLogoutSuccessAsync instead. Scheduled for removal in Umbraco 21.")]
    SignOutSuccessResult NotifyLogoutSuccess(IPrincipal currentUser, string? userId);

    /// <summary>
    /// Notifies the system that a forgot password request has been made for a back office user.
    /// </summary>
    /// <param name="currentUser">The principal representing the user initiating the notification, typically an administrator or system process.</param>
    /// <param name="userId">The unique identifier of the user for whom the password reset was requested.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    // TODO (V19): Remove the default implementation.
    Task NotifyForgotPasswordRequestedAsync(IPrincipal currentUser, string userId)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        NotifyForgotPasswordRequested(currentUser, userId);
#pragma warning restore CS0618 // Type or member is obsolete
        return Task.CompletedTask;
    }

    /// <summary>
    /// Notifies the system that a user's password has been changed as a result of completing the forgot password process.
    /// </summary>
    /// <param name="currentUser">The principal representing the user performing the notification action.</param>
    /// <param name="userId">The unique identifier of the user whose password was changed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    // TODO (V19): Remove the default implementation.
    Task NotifyForgotPasswordChangedAsync(IPrincipal currentUser, string userId)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        NotifyForgotPasswordChanged(currentUser, userId);
#pragma warning restore CS0618 // Type or member is obsolete
        return Task.CompletedTask;
    }

    /// <summary>
    /// Notifies the system that a logout operation has succeeded for a specified user.
    /// </summary>
    /// <param name="currentUser">The principal representing the user performing the logout.</param>
    /// <param name="userId">The optional identifier of the user who logged out; may be <c>null</c>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="SignOutSuccessResult"/> representing the outcome of the logout notification.</returns>
    // TODO (V19): Remove the default implementation.
    Task<SignOutSuccessResult> NotifyLogoutSuccessAsync(IPrincipal currentUser, string? userId)
#pragma warning disable CS0618 // Type or member is obsolete
        => Task.FromResult(NotifyLogoutSuccess(currentUser, userId));
#pragma warning restore CS0618 // Type or member is obsolete
}
