using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Install
{
    [UmbracoApiController]
    [AngularJsonOnlyConfiguration]
    [InstallAuthorize]
    [Area(Cms.Core.Constants.Web.Mvc.InstallArea)]
    public class InstallApiController : ControllerBase
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly InstallStatusTracker _installStatusTracker;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly InstallStepCollection _installSteps;
        private readonly ILogger<InstallApiController> _logger;
        private readonly IProfilingLogger _proflog;

        public InstallApiController(DatabaseBuilder databaseBuilder, IProfilingLogger proflog, ILogger<InstallApiController> logger,
            InstallHelper installHelper, InstallStepCollection installSteps, InstallStatusTracker installStatusTracker,
            IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
            _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
            _installSteps = installSteps;
            _installStatusTracker = installStatusTracker;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
            InstallHelper = installHelper;
            _logger = logger;
        }


        internal InstallHelper InstallHelper { get; }

        public bool PostValidateDatabaseConnection(DatabaseModel model)
        {
            var canConnect = _databaseBuilder.CanConnect(model.DatabaseType.ToString(), model.ConnectionString,
                model.Server, model.DatabaseName, model.Login, model.Password, model.IntegratedAuth);
            return canConnect;
        }

        /// <summary>
        ///     Gets the install setup.
        /// </summary>
        public InstallSetup GetSetup()
        {
            var setup = new InstallSetup();

            // TODO: Check for user/site token

            var steps = new List<InstallSetupStep>();

            var installSteps = _installSteps.GetStepsForCurrentInstallType().ToArray();

            //only get the steps that are targeting the current install type
            steps.AddRange(installSteps);
            setup.Steps = steps;

            _installStatusTracker.Initialize(setup.InstallId, installSteps);

            return setup;
        }

        public IEnumerable<Package> GetPackages()
        {
            var starterKits = InstallHelper.GetStarterKits();
            return starterKits;
        }

        [HttpPost]
        public async Task<ActionResult> CompleteInstall()
        {
            _umbracoApplicationLifetime.Restart();
            return NoContent();
        }

        /// <summary>
        ///     Installs.
        /// </summary>
        public async Task<ActionResult<InstallProgressResultModel>> PostPerformInstall(InstallInstructions installModel)
        {
            if (installModel == null) throw new ArgumentNullException(nameof(installModel));

            var status = InstallStatusTracker.GetStatus().ToArray();
            //there won't be any statuses returned if the app pool has restarted so we need to re-read from file.
            if (status.Any() == false)
            {
                status = _installStatusTracker.InitializeFromFile(installModel.InstallId).ToArray();
            }

            //create a new queue of the non-finished ones
            var queue = new Queue<InstallTrackingItem>(status.Where(x => x.IsComplete == false));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var step = _installSteps.GetAllSteps().Single(x => x.Name == item.Name);

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
                    var setupData = await ExecuteStepAsync(step, instruction);

                    // update the status
                    _installStatusTracker.SetComplete(installModel.InstallId, step.Name, setupData?.SavedStepData);

                    // determine's the next step in the queue and dequeue's any items that don't need to execute
                    var nextStep = IterateSteps(step, queue, installModel.InstallId, installModel);

                    // check if there's a custom view to return for this step
                    if (setupData != null && setupData.View.IsNullOrWhiteSpace() == false)
                    {
                        return new InstallProgressResultModel(false, step.Name, nextStep, setupData.View,
                            setupData.ViewModel);
                    }

                    return new InstallProgressResultModel(false, step.Name, nextStep);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during installation step {Step}",
                        step.Name);

                    if (ex is TargetInvocationException && ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    var installException = ex as InstallException;
                    if (installException != null)
                    {
                        return new ValidationErrorResult(new
                        {
                            view = installException.View,
                            model = installException.ViewModel,
                            message = installException.Message
                        });
                    }

                    return new ValidationErrorResult(new
                    {
                        step = step.Name,
                        view = "error",
                        message = ex.Message
                    });
                }
            }

            _installStatusTracker.Reset();
            return new InstallProgressResultModel(true, "", "");
        }

        private static object GetInstruction(InstallInstructions installModel, InstallTrackingItem item,
            InstallSetupStep step)
        {
            installModel.Instructions.TryGetValue(item.Name, out var instruction); // else null

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
        private string IterateSteps(InstallSetupStep current, Queue<InstallTrackingItem> queue, Guid installId,
            InstallInstructions installModel)
        {
            while (queue.Count > 0)
            {
                var item = queue.Peek();

                // if the current step restarts the app pool then we must simply return the next one in the queue,
                // we cannot peek ahead as the next step might rely on the app restart and therefore RequiresExecution
                // will rely on that too.
                if (current.PerformsAppRestart)
                    return item.Name;

                var step = _installSteps.GetAllSteps().Single(x => x.Name == item.Name);

                // if this step has any instructions then extract them
                var instruction = GetInstruction(installModel, item, step);

                // if the step requires execution then return its name
                if (StepRequiresExecution(step, instruction))
                    return step.Name;

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
        internal bool StepRequiresExecution(InstallSetupStep step, object instruction)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            var modelAttempt = instruction.TryConvertTo(step.StepType);
            if (!modelAttempt.Success)
            {
                throw new InvalidCastException(
                    $"Cannot cast/convert {step.GetType().FullName} into {step.StepType.FullName}");
            }

            var model = modelAttempt.Result;
            var genericStepType = typeof(InstallSetupStep<>);
            Type[] typeArgs = { step.StepType };
            var typedStepType = genericStepType.MakeGenericType(typeArgs);
            try
            {
                var method = typedStepType.GetMethods().Single(x => x.Name == "RequiresExecution");
                return (bool) method.Invoke(step, new[] { model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checking if step requires execution ({Step}) failed.",
                    step.Name);
                throw;
            }
        }

        // executes the step
        internal async Task<InstallSetupResult> ExecuteStepAsync(InstallSetupStep step, object instruction)
        {
            using (_proflog.TraceDuration<InstallApiController>($"Executing installation step: '{step.Name}'.",
                "Step completed"))
            {
                var modelAttempt = instruction.TryConvertTo(step.StepType);
                if (!modelAttempt.Success)
                {
                    throw new InvalidCastException(
                        $"Cannot cast/convert {step.GetType().FullName} into {step.StepType.FullName}");
                }

                var model = modelAttempt.Result;
                var genericStepType = typeof(InstallSetupStep<>);
                Type[] typeArgs = { step.StepType };
                var typedStepType = genericStepType.MakeGenericType(typeArgs);
                try
                {
                    var method = typedStepType.GetMethods().Single(x => x.Name == "ExecuteAsync");
                    var task = (Task<InstallSetupResult>) method.Invoke(step, new[] { model });
                    return await task;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Installation step {Step} failed.", step.Name);
                    throw;
                }
            }
        }
    }
}
