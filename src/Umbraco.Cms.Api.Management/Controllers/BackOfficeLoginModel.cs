using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management;

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

    public bool UserIsAlreadyLoggedIn { get; set; }
}
