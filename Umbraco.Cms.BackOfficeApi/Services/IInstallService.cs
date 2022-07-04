using Umbraco.Cms.Core.Install.NewInstallSteps;

namespace Umbraco.Cms.BackOfficeApi.Services;

public interface IInstallService
{
    public Task Install(InstallData model);

    public Task Upgrade();
}
