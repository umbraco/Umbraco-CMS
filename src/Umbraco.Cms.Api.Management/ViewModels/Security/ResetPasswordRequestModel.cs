using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

    /// <summary>
    /// Represents the data model used in API requests to reset a user's password.
    /// Contains the necessary information required to perform a password reset operation.
    /// </summary>
public class ResetPasswordRequestModel
{
    /// <summary>
    /// Gets or sets the email address associated with the password reset request.
    /// </summary>
    [Required]
    public string Email { get; set; } = string.Empty;
}
