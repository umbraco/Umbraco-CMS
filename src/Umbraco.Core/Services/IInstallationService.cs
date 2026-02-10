namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides functionality for logging Umbraco installation events and telemetry.
/// </summary>
public interface IInstallationService
{
    /// <summary>
    /// Logs an installation event with the provided installation details.
    /// </summary>
    /// <param name="installLog">The installation log containing details about the installation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LogInstall(InstallLog installLog);
}
