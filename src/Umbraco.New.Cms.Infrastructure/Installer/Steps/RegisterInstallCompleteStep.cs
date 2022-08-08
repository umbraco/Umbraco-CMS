using Umbraco.Cms.Infrastructure.Install;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class RegisterInstallCompleteStep : IInstallStep, IUpgradeStep
{
    private readonly InstallHelper _installHelper;

    public RegisterInstallCompleteStep(InstallHelper installHelper) => _installHelper = installHelper;

    public Task ExecuteAsync(InstallData _) => Execute();

    public Task ExecuteAsync() => Execute();

    private Task Execute() => _installHelper.SetInstallStatusAsync(true, string.Empty);

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
