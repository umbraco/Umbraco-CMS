namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for installation logging.
/// </summary>
[Obsolete("Installation logging is no longer supported and this interface will be removed in Umbraco 19.")]
public interface IInstallationRepository
{
    /// <summary>
    ///     Saves an installation log entry.
    /// </summary>
    /// <param name="installLog">The installation log to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    Task SaveInstallLogAsync(InstallLog installLog);
}
