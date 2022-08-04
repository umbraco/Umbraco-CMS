using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Services.Installer;

public interface IInstallService
{
    Task Install(InstallData model);
}
