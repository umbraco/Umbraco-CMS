using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Core.Services.Installer;

// TODO: Move me when post install step is created.
namespace Umbraco.Cms.ManagementApi.Services.Installer;

public class InstallService : IInstallService
{
    private readonly ILogger<InstallService> _logger;
    private readonly NewInstallStepCollection _installSteps;
    private readonly IRuntimeState _runtimeState;
    public InstallService(
        ILogger<InstallService> logger,
        NewInstallStepCollection installSteps,
        IRuntimeState runtimeState)
    {
        _logger = logger;
        _installSteps = installSteps;
        _runtimeState = runtimeState;
    }

    public async Task Install(InstallData model)
    {
        if (_runtimeState.Level != RuntimeLevel.Install)
        {
            throw new InvalidOperationException($"Runtime level must be Install to install but was: {_runtimeState.Level}");
        }

        IEnumerable<IInstallStep> steps = _installSteps.GetInstallSteps();
        await RunSteps(steps, model);
    }

    public async Task Upgrade()
    {
        if (_runtimeState.Level != RuntimeLevel.Upgrade)
        {
            throw new InvalidOperationException($"Runtime level must be Upgrade to upgrade but was: {_runtimeState.Level}");
        }

        // Need to figure out how to handle the install data, this is only needed when installing, not upgrading.
        var model = new InstallData();

        IEnumerable<IInstallStep> steps = _installSteps.GetUpgradeSteps();
        await RunSteps(steps, model);
    }

    private async Task RunSteps(IEnumerable<IInstallStep> steps, InstallData model)
    {
        foreach (IInstallStep step in steps)
        {
            var stepName = step.GetType().Name;
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
