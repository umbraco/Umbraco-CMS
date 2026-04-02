namespace Umbraco.Cms.Api.Management.ViewModels.Server;

/// <summary>
/// Represents the response model containing server configuration details returned by the API.
/// </summary>
public class ServerConfigurationResponseModel
{
    /// <summary>Gets or sets a value indicating whether password reset is allowed.</summary>
    public bool AllowPasswordReset { get; set; }

    /// <summary>
    /// Gets or sets the period in which version checks occur.
    /// The unit of the period (e.g., minutes, hours, days) depends on the implementation.
    /// </summary>
    public int VersionCheckPeriod { get; set; }

    /// <summary>Gets or sets a value indicating whether local login is allowed.</summary>
    public bool AllowLocalLogin { get; set; }

    /// <summary>
    /// Gets or sets the relative or absolute path to the Umbraco CSS file used by the application.
    /// </summary>
    public string UmbracoCssPath { get; set; } = string.Empty;
}
