using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Installer;

namespace Umbraco.New.Cms.Core.Services.Installer;

public class UpgradeService : IUpgradeService
{
    private readonly UpgradeStepCollection _upgradeSteps;
    private readonly IRuntimeState _runtimeState;
    private readonly ILogger<UpgradeService> _logger;

    public UpgradeService(
        UpgradeStepCollection upgradeSteps,
        IRuntimeState runtimeState,
        ILogger<UpgradeService> logger)
    {
        _upgradeSteps = upgradeSteps;
        _runtimeState = runtimeState;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Upgrade()
    {
        if (_runtimeState.Level != RuntimeLevel.Upgrade)
        {
            throw new InvalidOperationException(
                $"Runtime level must be Upgrade to upgrade but was: {_runtimeState.Level}");
        }

        try
        {
            await RunSteps();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Encountered an error when running the upgrade steps");
            throw;
        }
    }

    private async Task RunSteps()
    {
        foreach (IUpgradeStep step in _upgradeSteps)
        {
            var stepName = step.GetType().Name;
            _logger.LogInformation("Checking if {StepName} requires execution", stepName);
            if (await step.RequiresExecutionAsync() is false)
            {
                _logger.LogInformation("Skipping {StepName}", stepName);
                continue;
            }

            _logger.LogInformation("Running {StepName}", stepName);
            await step.ExecuteAsync();
            _logger.LogInformation("Finished {StepName}", stepName);
        }
    }
}
