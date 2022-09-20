using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Services.Installer;

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

    /// <inheritdoc/>
    public async Task Install(InstallData model)
    {
        if (_runtimeState.Level != RuntimeLevel.Install)
        {
            throw new InvalidOperationException($"Runtime level must be Install to install but was: {_runtimeState.Level}");
        }

        try
        {
            await RunSteps(model);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Encountered an error when running the install steps");
            throw;
        }
    }

    private async Task RunSteps(InstallData model)
    {
        foreach (IInstallStep step in _installSteps)
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
            _logger.LogInformation("Finished {StepName}", stepName);
        }
    }
}
