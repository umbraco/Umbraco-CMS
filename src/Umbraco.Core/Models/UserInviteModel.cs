namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the model used for inviting a new user to the system.
/// </summary>
/// <remarks>
///     Extends <see cref="UserCreateModel" /> with an optional message to include in the invitation.
/// </remarks>
public class UserInviteModel : UserCreateModel
{
    /// <summary>
    ///     Gets or sets an optional custom message to include in the invitation email.
    /// </summary>
    public string? Message { get; set; }
}
