using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Services.Installer;

public interface IInstallService
{
    public Task Install(InstallData model);

    public Task Upgrade();
}
