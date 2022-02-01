using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Web.Install.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Install.Controllers
{
    [AngularJsonOnlyConfiguration]
    [HttpInstallAuthorize]
    public class InstallApiController : ApiController
    {
        private readonly DatabaseBuilder _databaseBuilder;
        private readonly IProfilingLogger _proflog;
        private readonly InstallStepCollection _installSteps;
        private readonly ILogger _logger;

        public InstallApiController(DatabaseBuilder databaseBuilder, IProfilingLogger proflog, InstallHelper installHelper, InstallStepCollection installSteps)
        {
            _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
            _proflog = proflog ?? throw new ArgumentNullException(nameof(proflog));
            _installSteps = installSteps;
            InstallHelper = installHelper;
            _logger = _proflog;
        }

        internal InstallHelper InstallHelper { get; }

        public bool PostValidateDatabaseConnection(DatabaseModel model)
        {
            var canConnect = _databaseBuilder.CanConnect(model.DatabaseType.ToString(), model.ConnectionString, model.Server, model.DatabaseName, model.Login, model.Password, model.IntegratedAuth);
            return canConnect;
        }

        /// <summary>
        /// Gets the install setup.
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

            InstallStatusTracker.Initialize(setup.InstallId, installSteps);

            return setup;
        }

        public IEnumerable<Package> GetPackages()
        {
            var starterKits = InstallHelper.GetStarterKits();
            return starterKits;
        }

        /// <summary>
        /// Installs.
        /// </summary>
        public async Task<InstallProgressResultModel> PostPerformInstall(InstallInstructions installModel)
        {
            if (installModel == null) throw new ArgumentNullException(nameof(installModel));

            var status = InstallStatusTracker.GetStatus().ToArray();
            //there won't be any statuses returned if the app pool has restarted so we need to re-read from file.
            if (status.Any() == false)
            {
                status = InstallStatusTracker.InitializeFromFile(installModel.InstallId).ToArray();
            }

            //create a new queue of the non-finished ones
            var queue = new Queue<InstallTrackingItem>(status.Where(x => x.IsComplete == false));
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var step = _installSteps.GetAllSteps().Single(x => x.Name == item.Name);

                // if this step has any instructions then extract them
                installModel.Instructions.TryGetValue(item.Name, out var instruction); // else null

                // if this step doesn't require execution then continue to the next one, this is just a fail-safe check.
                if (StepRequiresExecution(step, instruction) == false)
                {
                    // set this as complete and continue
                    InstallStatusTracker.SetComplete(installModel.InstallId, item.Name);
                    continue;
                }

                try
                {
                    var setupData = await ExecuteStepAsync(step, instruction);

                    // update the status
                    InstallStatusTracker.SetComplete(installModel.InstallId, step.Name, setupData?.SavedStepData);

                    // determine's the next step in the queue and dequeue's any items that don't need to execute
                    var nextStep = IterateSteps(step, queue, installModel.InstallId, installModel);

                    // check if there's a custom view to return for this step
                    if (setupData != null && setupData.View.IsNullOrWhiteSpace() == false)
                    {
                        return new InstallProgressResultModel(false, step.Name, nextStep, setupData.View, setupData.ViewModel);
                    }

                    return new InstallProgressResultModel(false, step.Name, nextStep);
                }
                catch (Exception ex)
                {

                    _logger.Error<InstallApiController, string>(ex, "An error occurred during installation step {Step}", step.Name);

                    if (ex is TargetInvocationException && ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                    }

                    var installException = ex as InstallException;
                    if (installException != null)
                    {
                        throw new HttpResponseException(Request.CreateValidationErrorResponse(new
                        {
                            view = installException.View,
                            model = installException.ViewModel,
                            message = installException.Message
                        }));
                    }

                    throw new HttpResponseException(Request.CreateValidationErrorResponse(new
                    {
                        step = step.Name,
                        view = "error",
                        message = ex.Message
                    }));
                }
            }

            InstallStatusTracker.Reset();
            return new InstallProgressResultModel(true, "", "");
        }

        /// <summary>
        /// We'll peek ahead and check if it's RequiresExecution is returning true. If it
        /// is not, we'll dequeue that step and peek ahead again (recurse)
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
                var item = queue.Peek();

                // if the current step restarts the app pool then we must simply return the next one in the queue,
                // we cannot peek ahead as the next step might rely on the app restart and therefore RequiresExecution
                // will rely on that too.
                if (current.PerformsAppRestart)
                    return item.Name;

                var step = _installSteps.GetAllSteps().Single(x => x.Name == item.Name);

                // if this step has any instructions then extract them
                JToken instruction;
                installModel.Instructions.TryGetValue(item.Name, out instruction); // else null

                // if the step requires execution then return its name
                if (StepRequiresExecution(step, instruction))
                    return step.Name;

                // no longer requires execution, could be due to a new config change during installation
                // dequeue
                queue.Dequeue();

                // complete
                InstallStatusTracker.SetComplete(installId, step.Name);

                // and continue
                current = step;
            }

            return string.Empty;
        }

        // determines whether the step requires execution
        internal bool StepRequiresExecution(InstallSetupStep step, JToken instruction)
        {
            var model = instruction?.ToObject(step.StepType);
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
                _logger.Error<InstallApiController, string>(ex, "Checking if step requires execution ({Step}) failed.", step.Name);
                throw;
            }
        }

        // executes the step
        internal async Task<InstallSetupResult> ExecuteStepAsync(InstallSetupStep step, JToken instruction)
        {
            using (_proflog.TraceDuration<InstallApiController>($"Executing installation step: '{step.Name}'.", "Step completed"))
            {
                var model = instruction?.ToObject(step.StepType);
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
                    _logger.Error<InstallApiController, string>(ex, "Installation step {Step} failed.", step.Name);
                    throw;
                }
            }
        }
    }
}
