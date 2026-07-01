using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the request model used to verify an invited user.
/// </summary>
public class VerifyInviteUserRequestModel
{
    /// <summary>
    /// Gets or sets the user reference by ID.
    /// </summary>
    public required ReferenceByIdModel User { get; set; }

    /// <summary>
    /// Gets or sets the token used to verify the invited user.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
}
