using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Cms.Api.Management.ViewModels.Security;

/// <summary>
/// Represents a request model to unlink a login from a user account.
/// </summary>
public class UnLinkLoginRequestModel
{
    /// <summary>
    /// Gets or sets the login provider to unlink.
    /// </summary>
    [Required]
    public required string LoginProvider { get; set; }

    /// <summary>
    /// Gets or sets the provider key for the login to unlink.
    /// </summary>
    [Required]
    public required string ProviderKey { get; set; }
}
