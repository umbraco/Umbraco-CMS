using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Install;

[Obsolete("Will be replaced with a new API controller in the new backoffice api")]
[UmbracoApiController]
[AngularJsonOnlyConfiguration]
[InstallAuthorize]
[Area(Constants.Web.Mvc.InstallArea)]
public class InstallApiController : ControllerBase
{
    private readonly IBackOfficeSignInManager _backOfficeSignInManager;
    private readonly IBackOfficeUserManager _backOfficeUserManager;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly InstallStatusTracker _installStatusTracker;
    private readonly InstallStepCollection _installSteps;
    private readonly ILogger<InstallApiController> _logger;
    private readonly IProfilingLogger _proflog;
    private readonly IRuntime _runtime;

    public InstallApiController(
        DatabaseBuilder databaseBuilder,
        IProfilingLogger proflog,
        ILogger<InstallApiController> logger,
        InstallHelper installHelper,
        InstallStepCollection installSteps,
        InstallStatusTracker installStatusTracker,
        IRuntime runtime,
        IBackOfficeUserManager backOfficeUserManager,
        IBackOfficeSignInManager backOfficeSignInManager)
    {
        _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
        _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
        _installSteps = installSteps;
        _installStatusTracker = installStatusTracker;
        _runtime = runtime;
        _backOfficeUserManager = backOfficeUserManager;
        _backOfficeSignInManager = backOfficeSignInManager;
        InstallHelper = installHelper;
        _logger = logger;
    }


    internal InstallHelper InstallHelper { get; }

    public bool PostValidateDatabaseConnection(DatabaseModel databaseSettings)
    {
        if (_runtime.State.Level != RuntimeLevel.Install)
        {
            return false;
        }

        return _databaseBuilder.ConfigureDatabaseConnection(databaseSettings, true);
    }

    /// <summary>
    /// Gets the install setup.
    /// </summary>
    public InstallSetup GetSetup()
    {
        // Only get the steps that are targeting the current install type
        var setup = new InstallSetup
        {
            Steps = _installSteps.GetStepsForCurrentInstallType().ToList()
        };

        _installStatusTracker.Initialize(setup.InstallId, setup.Steps);

        return setup;
    }

    [HttpPost]
    public async Task<ActionResult> CompleteInstall()
    {
        RuntimeLevel levelBeforeRestart = _runtime.State.Level;

        await _runtime.RestartAsync();

        if (levelBeforeRestart == RuntimeLevel.Install)
        {
            BackOfficeIdentityUser identityUser = await _backOfficeUserManager.FindByIdAsync(Core.Constants.Security.SuperUserIdAsString);
            _backOfficeSignInManager.SignInAsync(identityUser, false);
        }

        return NoContent();
    }

    public async Task<ActionResult<InstallProgressResultModel>> PostPerformInstall(InstallInstructions installModel)
    {
        if (installModel == null)
        {
            throw new ArgumentNullException(nameof(installModel));
        }

        // There won't be any statuses returned if the app pool has restarted so we need to re-read from file
        InstallTrackingItem[] status = InstallStatusTracker.GetStatus().ToArray();
        if (status.Any() == false)
        {
            status = _installStatusTracker.InitializeFromFile(installModel.InstallId).ToArray();
        }

        // Create a new queue of the non-finished ones
        var queue = new Queue<InstallTrackingItem>(status.Where(x => x.IsComplete == false));
        while (queue.Count > 0)
        {
            InstallTrackingItem item = queue.Dequeue();
            InstallSetupStep step = _installSteps.GetAllSteps().Single(x => x.Name == item.Name);

            // if this step has any instructions then extract them
            var instruction = GetInstruction(installModel, item, step);

            // if this step doesn't require execution then continue to the next one, this is just a fail-safe check.
            if (StepRequiresExecution(step, instruction) == false)
            {
                // set this as complete and continue
                _installStatusTracker.SetComplete(installModel.InstallId, item.Name);
                continue;
            }

            try
            {
                InstallSetupResult? setupData = await ExecuteStepAsync(step, instruction);

                // update the status
                _installStatusTracker.SetComplete(installModel.InstallId, step.Name, setupData?.SavedStepData);

                // determine's the next step in the queue and dequeue's any items that don't need to execute
                var nextStep = IterateSteps(step, queue, installModel.InstallId, installModel);
                bool processComplete = string.IsNullOrEmpty(nextStep) && InstallStatusTracker.GetStatus().All(x => x.IsComplete);

                // check if there's a custom view to return for this step
                if (setupData != null && setupData.View.IsNullOrWhiteSpace() == false)
                {
                    return new InstallProgressResultModel(processComplete, step.Name, nextStep, setupData.View, setupData.ViewModel);
                }

                return new InstallProgressResultModel(processComplete, step.Name, nextStep);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during installation step {Step}", step.Name);

                if (ex is TargetInvocationException && ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                if (ex is InstallException installException)
                {
                    return new ValidationErrorResult(new
                    {
                        view = installException.View,
                        model = installException.ViewModel,
                        message = installException.Message
                    });
                }

                return new ValidationErrorResult(new { step = step.Name, view = "error", message = ex.Message });
            }
        }

        _installStatusTracker.Reset();
        return new InstallProgressResultModel(true, string.Empty, string.Empty);
    }

    private static object? GetInstruction(InstallInstructions installModel, InstallTrackingItem item, InstallSetupStep step)
    {
        object? instruction = null;
        installModel.Instructions?.TryGetValue(item.Name, out instruction); // else null

        if (instruction is JObject jObject)
        {
            instruction = jObject?.ToObject(step.StepType);
        }

        return instruction;
    }

    /// <summary>
    ///     We'll peek ahead and check if it's RequiresExecution is returning true. If it
    ///     is not, we'll dequeue that step and peek ahead again (recurse)
    /// </summary>
    /// <param name="current"></param>
    /// <param name="queue"></param>
    /// <param name="installId"></param>
    /// <param name="installModel"></param>
    /// <returns></returns>
    private string IterateSteps(InstallSetupStep current, Queue<InstallTrackingItem> queue, Guid installId, InstallInstructions installModel)
    {
        while (queue.Count > 0)
        {
            InstallTrackingItem item = queue.Peek();

            // if the current step restarts the app pool then we must simply return the next one in the queue,
            // we cannot peek ahead as the next step might rely on the app restart and therefore RequiresExecution
            // will rely on that too.
            if (current.PerformsAppRestart)
            {
                return item.Name;
            }

            InstallSetupStep step = _installSteps.GetAllSteps().Single(x => x.Name == item.Name);

            // if this step has any instructions then extract them
            var instruction = GetInstruction(installModel, item, step);

            // if the step requires execution then return its name
            if (StepRequiresExecution(step, instruction))
            {
                return step.Name;
            }

            // no longer requires execution, could be due to a new config change during installation
            // dequeue
            queue.Dequeue();

            // complete
            _installStatusTracker.SetComplete(installId, step.Name);

            // and continue
            current = step;
        }

        return string.Empty;
    }

    // determines whether the step requires execution
    internal bool StepRequiresExecution(InstallSetupStep step, object? instruction)
    {
        if (step == null)
        {
            throw new ArgumentNullException(nameof(step));
        }

        Attempt<object?> modelAttempt = instruction.TryConvertTo(step.StepType);
        if (!modelAttempt.Success)
        {
            throw new InvalidCastException($"Cannot cast/convert {step.GetType().FullName} into {step.StepType.FullName}");
        }

        var model = modelAttempt.Result;
        Type genericStepType = typeof(InstallSetupStep<>);
        Type[] typeArgs = { step.StepType };
        Type typedStepType = genericStepType.MakeGenericType(typeArgs);
        try
        {
            MethodInfo method = typedStepType.GetMethods().Single(x => x.Name == "RequiresExecution");
            var result = (bool?)method.Invoke(step, new[] { model });
            return result ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checking if step requires execution ({Step}) failed.", step.Name);
            throw;
        }
    }

    // executes the step
    internal async Task<InstallSetupResult> ExecuteStepAsync(InstallSetupStep step, object? instruction)
    {
        using (_proflog.TraceDuration<InstallApiController>($"Executing installation step: '{step.Name}'.", "Step completed"))
        {
            Attempt<object?> modelAttempt = instruction.TryConvertTo(step.StepType);
            if (!modelAttempt.Success)
            {
                throw new InvalidCastException($"Cannot cast/convert {step.GetType().FullName} into {step.StepType.FullName}");
            }

            var model = modelAttempt.Result;
            Type genericStepType = typeof(InstallSetupStep<>);
            Type[] typeArgs = { step.StepType };
            Type typedStepType = genericStepType.MakeGenericType(typeArgs);
            try
            {
                MethodInfo method = typedStepType.GetMethods().Single(x => x.Name == "ExecuteAsync");
                var task = (Task<InstallSetupResult>?)method.Invoke(step, new[] { model });
                return await task!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Installation step {Step} failed.", step.Name);
                throw;
            }
        }
    }
}
