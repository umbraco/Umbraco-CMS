namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Represents the data required to link an external login to a user account.
/// </summary>
public class LinkLoginRequestModel
{
    /// <summary>
    /// Gets or sets the provider name for the link login request.
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// Gets or sets the unique link key used for login.
    /// </summary>
    public required Guid LinkKey { get; set; }
}
