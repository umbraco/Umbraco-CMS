using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides functionality to send forgot password messages to users.
/// </summary>
public interface IUserForgotPasswordSender
{
    /// <summary>
    ///     Sends a forgot password message to the user.
    /// </summary>
    /// <param name="message">The forgot password message containing user and reset details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendForgotPassword(UserForgotPasswordMessage message);

    /// <summary>
    ///     Determines whether the sender is configured and able to send messages.
    /// </summary>
    /// <returns><c>true</c> if the sender can send messages; otherwise, <c>false</c>.</returns>
    [Obsolete("Please use IsPasswordResetConfigured to check configuration only, or IsPasswordResetAvailableAsync to also check that messages can currently be delivered. Scheduled for removal in Umbraco 19.")]
    bool CanSend();

    /// <summary>
    ///     Determines whether password reset is enabled and the sender is configured to send messages.
    /// </summary>
    /// <returns><c>true</c> if password reset is configured; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     This checks configuration only and performs no I/O, so it is suitable for deciding whether to offer password reset.
    ///     Use <see cref="IsPasswordResetAvailableAsync"/> immediately before sending to also check that messages can be delivered.
    /// </remarks>
    // TODO (V19): Remove the default implementation when CanSend is removed.
    bool IsPasswordResetConfigured()
#pragma warning disable CS0618 // Type or member is obsolete
        => CanSend();
#pragma warning restore CS0618 // Type or member is obsolete

    /// <summary>
    ///     Determines whether password reset is configured and the sender can currently deliver messages.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><c>true</c> if password reset messages can currently be delivered; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     Implementations may perform network I/O, so prefer <see cref="IsPasswordResetConfigured"/> unless a message is
    ///     about to be sent.
    /// </remarks>
    // TODO (V19): Remove the default implementation.
    Task<bool> IsPasswordResetAvailableAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(IsPasswordResetConfigured());
}
