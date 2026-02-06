using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the message data for a forgot password notification to a user.
/// </summary>
public class UserForgotPasswordMessage
{
    /// <summary>
    ///     Gets or sets the URI for the password reset page.
    /// </summary>
    public required Uri ForgotPasswordUri { get; set; }

    /// <summary>
    ///     Gets or sets the user who will receive the forgot password message.
    /// </summary>
    public required IUser Recipient { get; set; }
}
