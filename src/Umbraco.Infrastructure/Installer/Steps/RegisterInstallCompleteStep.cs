using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Infrastructure.Install;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

public class RegisterInstallCompleteStep : StepBase, IInstallStep, IUpgradeStep
{
    [Obsolete("Please use the constructor without parameters. Scheduled for removal in Umbraco 19.")]
    public RegisterInstallCompleteStep(InstallHelper installHelper)
        : this()
    {
    }

    public RegisterInstallCompleteStep()
    {
    }

    public Task<Attempt<InstallationResult>> ExecuteAsync(InstallData _) => Execute();

    public Task<Attempt<InstallationResult>> ExecuteAsync() => Execute();

    private Task<Attempt<InstallationResult>> Execute() => Task.FromResult(Success());

    public Task<bool> RequiresExecutionAsync(InstallData _) => ShouldExecute();

    public Task<bool> RequiresExecutionAsync() => ShouldExecute();

    private static Task<bool> ShouldExecute() => Task.FromResult(true);
}
