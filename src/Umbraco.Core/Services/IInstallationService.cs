using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IInstallationService
    {
        Task Install(InstallLog installLog);
    }
}
