namespace Umbraco.Cms.Web.Website.Models;

/// <summary>
/// View model for the standalone basic auth login page.
/// </summary>
public class BasicAuthLoginModel
{
    /// <summary>
    /// Gets or sets the local URL to redirect to after successful login.
    /// </summary>
    public string? ReturnPath { get; set; }

    /// <summary>
    /// Gets or sets an error message to display on the login form.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
