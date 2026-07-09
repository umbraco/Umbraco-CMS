using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Service for logging installation information.
/// </summary>
public class InstallationService : IInstallationService
{
    private readonly IInstallationRepository _installationRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InstallationService" /> class.
    /// </summary>
    /// <param name="installationRepository">The installation repository.</param>
    public InstallationService(IInstallationRepository installationRepository) =>
        _installationRepository = installationRepository;

    /// <inheritdoc />
    public async Task LogInstall(InstallLog installLog) =>
        await _installationRepository.SaveInstallLogAsync(installLog);
}
