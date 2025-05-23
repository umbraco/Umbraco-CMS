using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Infrastructure.Install;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

public class RegisterInstallCompleteStep : StepBase, IInstallStep, IUpgradeStep
{
    private readonly InstallHelper _installHelper;

    public RegisterInstallCompleteStep(InstallHelper installHelper) => _installHelper = installHelper;

    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    private async Task<Attempt<InstallationResult>> Execute()
    {
        await _installHelper.SetInstallStatusAsync(true, string.Empty);
        return Success();
    }

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
