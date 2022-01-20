using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Services
{
    public interface IInstallationService
    {
        Task LogInstall(InstallLog installLog);
    }
}
