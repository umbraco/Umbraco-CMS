using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

public abstract class InstallStepBase : IInstallStep
{
    protected Attempt<InstallationResult> FailWithMessage(string message)
        => Attempt<InstallationResult>.Fail(new InstallationResult { ErrorMessage = message });


    protected Attempt<InstallationResult> Success() => Attempt<InstallationResult>.Succeed(new InstallationResult());

    public abstract Task<Attempt<InstallationResult>> ExecuteAsync(InstallData model);

    public abstract Task<bool> RequiresExecutionAsync(InstallData model);
}
