using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.Installer;

/// <summary>
/// Provides functionality for performing a fresh installation of Umbraco CMS.
/// </summary>
/// <remarks>
/// This service orchestrates the installation process by executing all registered install steps
/// in the <see cref="NewInstallStepCollection"/>. It validates that the runtime is in the correct
/// state before proceeding with installation.
/// </remarks>
public class InstallService : IInstallService
{
    private readonly ILogger<InstallService> _logger;
    private readonly NewInstallStepCollection _installSteps;
    private readonly IRuntimeState _runtimeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="InstallService"/> class.
    /// </summary>
    /// <param name="logger">The logger for recording installation progress and errors.</param>
    /// <param name="installSteps">The collection of installation steps to execute.</param>
    /// <param name="runtimeState">The runtime state used to verify the system is ready for installation.</param>
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
    public async Task<Attempt<InstallationResult?, InstallOperationStatus>> InstallAsync(InstallData model)
    {
        if (_runtimeState.Level != RuntimeLevel.Install)
        {
            throw new InvalidOperationException($"Runtime level must be Install to install but was: {_runtimeState.Level}");
        }

        try
        {
            Attempt<InstallationResult?> result = await RunStepsAsync(model);
            return result.Success
                ? Attempt.SucceedWithStatus(InstallOperationStatus.Success, result.Result)
                : Attempt.FailWithStatus(InstallOperationStatus.InstallFailed, result.Result);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Encountered an unexpected error when running the install steps");
            throw;
        }
    }

    private async Task<Attempt<InstallationResult?>> RunStepsAsync(InstallData model)
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
            Attempt<InstallationResult> result = await step.ExecuteAsync(model);

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
