using Umbraco.Cms.Core.Models.RedirectUrlManagement;

namespace Umbraco.Cms.Api.Management.ViewModels.RedirectUrlManagement;

/// <summary>
/// Represents a response model containing the status information for a redirect URL in the Umbraco CMS Management API.
/// </summary>
public class RedirectUrlStatusResponseModel
{
    /// <summary>
    /// Gets or sets the current status of the redirect URL, represented by the <see cref="RedirectStatus"/> enumeration.
    /// </summary>
    public RedirectStatus Status { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is an administrator.
    /// </summary>
    public bool UserIsAdmin { get; set; }
}
