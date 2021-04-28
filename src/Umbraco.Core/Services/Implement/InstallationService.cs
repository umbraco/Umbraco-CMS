using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Services.Implement
{
    public class InstallationService : IInstallationService
    {
        private readonly IInstallationRepository _installationRepository;

        public InstallationService(IInstallationRepository installationRepository)
        {
            _installationRepository = installationRepository;
        }

        public async Task LogInstall(InstallLog installLog)
        {
            await _installationRepository.SaveInstallLogAsync(installLog);
        }
    }
}
