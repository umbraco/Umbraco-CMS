namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Request model used to verify a password reset token.
/// </summary>
public class VerifyResetPasswordTokenRequestModel
{
    /// <summary>
    /// Gets or sets the user to verify, referenced by ID.
    /// </summary>
    public required ReferenceByIdModel User { get; set; }

    /// <summary>
    /// Gets or sets the reset code used to verify the password reset request.
    /// </summary>
    public required string ResetCode { get; set; }
}
