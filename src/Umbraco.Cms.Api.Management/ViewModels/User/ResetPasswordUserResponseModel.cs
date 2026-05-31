namespace Umbraco.Cms.Api.Management.ViewModels.User;

/// <summary>
/// Represents the response model returned after a user password reset operation.
/// Typically contains information about the outcome of the reset request.
/// </summary>
public class ResetPasswordUserResponseModel
{
    /// <summary>
    /// Gets or sets the token used to reset the user's password.
    /// </summary>
    public string? ResetPassword { get; set; }
}
