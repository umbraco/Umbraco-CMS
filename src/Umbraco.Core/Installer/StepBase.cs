using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Installer;

public abstract class StepBase
{
    protected Attempt<InstallationResult> FailWithMessage(string message)
        => Attempt<InstallationResult>.Fail(new InstallationResult { ErrorMessage = message });


    protected Attempt<InstallationResult> Success() => Attempt<InstallationResult>.Succeed(new InstallationResult());
}
