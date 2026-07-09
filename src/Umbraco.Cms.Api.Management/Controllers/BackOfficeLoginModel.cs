using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management;

/// <summary>
/// Represents a model containing the credentials required for logging into the Umbraco back office.
/// </summary>
[BindProperties]
public class BackOfficeLoginModel
{
    /// <summary>
    /// Gets or sets the value of the "ReturnUrl" query parameter or defaults to the configured Umbraco directory.
    /// </summary>
    [FromQuery(Name = "ReturnUrl")]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// The configured Umbraco directory.
    /// </summary>
    public string? UmbracoUrl { get; set; }

    /// <summary>
    /// Indicates whether the user is already logged in to the back office.
    /// </summary>
    public bool UserIsAlreadyLoggedIn { get; set; }
}
