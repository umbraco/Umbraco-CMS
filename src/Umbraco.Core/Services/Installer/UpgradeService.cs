using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Installer;

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
    public async Task<Attempt<InstallationResult?, UpgradeOperationStatus>> UpgradeAsync()
    {
        if (_runtimeState.Level != RuntimeLevel.Upgrade)
        {
            throw new InvalidOperationException(
                $"Runtime level must be Upgrade to upgrade but was: {_runtimeState.Level}");
        }

        try
        {
            Attempt<InstallationResult?> result = await RunStepsAsync();
            return result.Success
                ? Attempt.SucceedWithStatus(UpgradeOperationStatus.Success, result.Result)
                : Attempt.FailWithStatus(UpgradeOperationStatus.UpgradeFailed, result.Result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Encountered an unexpected error when running the upgrade steps");
            throw;
        }
    }

    private async Task<Attempt<InstallationResult?>> RunStepsAsync()
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
            Attempt<InstallationResult> result = await step.ExecuteAsync();

            if (result.Success is false)
            {
                if (result.Result?.ErrorMessage is not null)
                {
                    _logger.LogError("Failed {StepName}, with the message: {Message}", stepName, result.Result?.ErrorMessage);
                }
                else
                {
                    _logger.LogError("Failed {StepName}", stepName);
                }

                return Attempt.Fail(result.Result);
            }

            _logger.LogInformation("Finished {StepName}", stepName);
        }

        return Attempt<InstallationResult?>.Succeed();
    }
}
