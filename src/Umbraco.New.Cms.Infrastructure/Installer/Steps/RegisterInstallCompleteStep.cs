using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class RegisterInstallCompleteStep : IInstallStep
{
    private readonly InstallHelper _installHelper;

    public RegisterInstallCompleteStep(InstallHelper installHelper) => _installHelper = installHelper;

    public InstallationType InstallationTypeTarget => InstallationType.NewInstall | InstallationType.Upgrade;

    public Task ExecuteAsync(InstallData model) => _installHelper.SetInstallStatusAsync(true, string.Empty);

    public Task<bool> RequiresExecutionAsync(InstallData model) => Task.FromResult(true);
}
