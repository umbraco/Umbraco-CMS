using System;
using System.Threading.Tasks;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IInstallationRepository
    {
        Task SaveInstallLogAsync(InstallLog installLog);
    }
}
