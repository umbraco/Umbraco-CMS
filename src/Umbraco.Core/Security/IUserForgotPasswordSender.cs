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
    bool CanSend();
}
