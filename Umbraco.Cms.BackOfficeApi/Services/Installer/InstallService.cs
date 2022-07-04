using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Installer.Steps;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Core.Services.Installer;

// TODO: Move me when post install step is created.
namespace Umbraco.Cms.BackOfficeApi.Services.Installer;

public class InstallService : IInstallService
{
    private readonly ILogger<InstallService> _logger;
    private readonly NewInstallStepCollection _installSteps;
    private readonly IRuntimeState _runtimeState;
    private readonly IRuntime _runtime;
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;

    public InstallService(
        ILogger<InstallService> logger,
        NewInstallStepCollection installSteps,
        IRuntimeState runtimeState,
        IRuntime runtime,
        IBackOfficeSignInManager backOfficeSignInManager,
        IBackOfficeUserManager backOfficeUserManager)
    {
        _logger = logger;
        _installSteps = installSteps;
        _runtimeState = runtimeState;
        _runtime = runtime;
        _backOfficeSignInManager = backOfficeSignInManager;
        _backOfficeUserManager = backOfficeUserManager;
    }

    public async Task Install(InstallData model)
    {
        if (_runtimeState.Level != RuntimeLevel.Install)
        {
            throw new InvalidOperationException($"Runtime level must be Install to install but was: {_runtimeState.Level}");
        }

        IEnumerable<NewInstallSetupStep> steps = _installSteps.GetInstallSteps();
        await RunSteps(steps, model);

        await _runtime.RestartAsync();

        // Sign the newly created user in (Not sure if we want this separately in the future?
        BackOfficeIdentityUser identityUser =
            await _backOfficeUserManager.FindByIdAsync(Constants.Security.SuperUserIdAsString);
        await _backOfficeSignInManager.SignInAsync(identityUser, false);
    }

    public async Task Upgrade()
    {
        // Need to figure out how to handle the install data, this is only needed when installing, not upgrading.
        var model = new InstallData();

        IEnumerable<NewInstallSetupStep> steps = _installSteps.GetUpgradeSteps();
        await RunSteps(steps, model);

        await _runtime.RestartAsync();
    }

    private async Task RunSteps(IEnumerable<NewInstallSetupStep> steps, InstallData model)
    {
        foreach (NewInstallSetupStep step in steps)
        {
            var stepName = step.Name;
            _logger.LogInformation("Checking if {StepName} requires execution", stepName);
            if (await step.RequiresExecutionAsync(model) is false)
            {
                _logger.LogInformation("Skipping {StepName}", stepName);
                continue;
            }

            _logger.LogInformation("Running {StepName}", stepName);
            await step.ExecuteAsync(model);
        }
    }
}
