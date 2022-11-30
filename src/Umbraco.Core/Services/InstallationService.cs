using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Core.Services;

public class InstallationService : IInstallationService
{
    private readonly IInstallationRepository _installationRepository;

    public InstallationService(IInstallationRepository installationRepository) =>
        _installationRepository = installationRepository;

    public async Task LogInstall(InstallLog installLog) =>
        await _installationRepository.SaveInstallLogAsync(installLog);
}
