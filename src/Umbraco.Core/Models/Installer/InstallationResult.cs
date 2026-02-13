namespace Umbraco.Cms.Core.Models.Installer;

/// <summary>
///     Represents the result of an installation operation.
/// </summary>
public class InstallationResult
{
    /// <summary>
    /// Gets ore sets a string specifying why the installation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
