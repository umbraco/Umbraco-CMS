namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents a request model used by the management API to resend an invitation email to a user.
/// </summary>
public class ResendInviteUserRequestModel
{
    /// <summary>
    /// Gets or sets the user reference by ID for whom the invite is to be resent.
    /// </summary>
    public required ReferenceByIdModel User { get; set; }

    /// <summary>
    /// Gets or sets the message to include when resending the invite.
    /// </summary>
    public string? Message { get; set; }
}
