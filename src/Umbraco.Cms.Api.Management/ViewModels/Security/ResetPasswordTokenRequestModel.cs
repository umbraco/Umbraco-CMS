using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Request model for resetting a user's password with a token.
/// </summary>
public class ResetPasswordTokenRequestModel : VerifyResetPasswordTokenRequestModel
{
    /// <summary>
    /// Gets or sets the new password for the reset password token request.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
