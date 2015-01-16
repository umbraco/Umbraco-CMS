using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.Models;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Install.Controllers
{
    [AngularJsonOnlyConfiguration]
    [HttpInstallAuthorize]
    public class InstallApiController : ApiController
    {
        protected InstallApiController()
            : this(UmbracoContext.Current)
        {

        }

        protected InstallApiController(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; private set; }

        public ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        private InstallHelper _helper;
        internal InstallHelper InstallHelper
        {
            get
            {
                return _helper ?? (_helper = new InstallHelper(UmbracoContext));
            }
        }

        public bool PostValidateDatabaseConnection(DatabaseModel model)
        {
            var dbHelper = new DatabaseHelper();
            var canConnect = dbHelper.CheckConnection(model, ApplicationContext);
            return canConnect;
        }

        /// <summary>
        /// Gets the install setup
        /// </summary>
        /// <returns></returns>
        public InstallSetup GetSetup()
        {
            var setup = new InstallSetup();

            //TODO: Check for user/site token

            var steps = new List<InstallSetupStep>();

            var installSteps = InstallHelper.GetStepsForCurrentInstallType().ToArray();

            //only get the steps that are targetting the current install type
            steps.AddRange(installSteps);
            setup.Steps = steps;

            InstallStatusTracker.Initialize(setup.InstallId, installSteps);

            return setup;
        }
        
        public IEnumerable<Package> GetPackages()
        {
            var installHelper = new InstallHelper(UmbracoContext);
            var starterKits = installHelper.GetStarterKits();
            return starterKits;
        }

        /// <summary>
        /// Does the install
        /// </summary>
        /// <returns></returns>
        public InstallProgressResultModel PostPerformInstall(InstallInstructions installModel)
        {
            if (installModel == null) throw new ArgumentNullException("installModel");

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
                var stepStatus = queue.Dequeue();
                
                var step = InstallHelper.GetAllSteps().Single(x => x.Name == stepStatus.Name);

                JToken instruction = null;
                //If this step has any instructions then extract them
                if (installModel.Instructions.Any(x => x.Key == step.Name))
                {
                    instruction = installModel.Instructions[step.Name];
                }
                
                //If this step doesn't require execution then continue to the next one, this is just a fail-safe check.
                if (StepRequiresExecution(step, instruction) == false)
                {
                    //set this as complete and continue
                    InstallStatusTracker.SetComplete(installModel.InstallId, stepStatus.Name, null);
                    continue;
                }

                try
                {
                    var setupData = ExecuteStep(step, instruction);

                    //update the status
                    InstallStatusTracker.SetComplete(installModel.InstallId, step.Name, setupData != null ? setupData.SavedStepData : null);

                    //Determine's the next step in the queue and dequeue's any items that don't need to execute
                    var nextStep = IterateNextRequiredStep(step, queue, installModel.InstallId, installModel);
                    
                    //check if there's a custom view to return for this step
                    if (setupData != null && setupData.View.IsNullOrWhiteSpace() == false)
                    {
                        return new InstallProgressResultModel(false, step.Name, nextStep, setupData.View, setupData.ViewModel);
                    }

                    return new InstallProgressResultModel(false, step.Name, nextStep);
                }
                catch (Exception ex)
                {

                    LogHelper.Error<InstallApiController>("An error occurred during installation step " + step.Name, ex);

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
        private string IterateNextRequiredStep(InstallSetupStep current, Queue<InstallTrackingItem> queue, Guid installId, InstallInstructions installModel)
        {
            if (queue.Count > 0)
            {
                var next = queue.Peek();
                var step = InstallHelper.GetAllSteps().Single(x => x.Name == next.Name);

                //If the current step restarts the app pool then we must simply return the next one in the queue, 
                // we cannot peek ahead as the next step might rely on the app restart and therefore RequiresExecution 
                // will rely on that too.                
                if (current.PerformsAppRestart)
                {                    
                    return step.Name;
                }

                JToken instruction = null;
                //If this step has any instructions then extract them
                if (installModel.Instructions.Any(x => x.Key == step.Name))
                {
                    instruction = installModel.Instructions[step.Name];
                }

                //if the step requires execution then return it's name
                if (StepRequiresExecution(step, instruction))
                {
                    return step.Name;
                }

                //this step no longer requires execution, this could be due to a new config change during installation,
                // so we'll dequeue this one from the queue and recurse
                queue.Dequeue();

                //set this as complete
                InstallStatusTracker.SetComplete(installId, step.Name, null);

                //recurse
                return IterateNextRequiredStep(step, queue, installId, installModel);
            }

            //there is no more steps
            return string.Empty;
        }

        /// <summary>
        /// Check if the step requires execution
        /// </summary>
        /// <param name="step"></param>
        /// <param name="instruction"></param>
        /// <returns></returns>
        internal bool StepRequiresExecution(InstallSetupStep step, JToken instruction)
        {
            var model = instruction == null ? null : instruction.ToObject(step.StepType);
            var genericStepType = typeof(InstallSetupStep<>);
            Type[] typeArgs = { step.StepType };
            var typedStepType = genericStepType.MakeGenericType(typeArgs);
            try
            {
                var method = typedStepType.GetMethods().Single(x => x.Name == "RequiresExecution");
                return (bool)method.Invoke(step, new object[] { model });
            }
            catch (Exception ex)
            {
                LogHelper.Error<InstallApiController>("Checking if step requires execution (" + step.Name + ") failed.", ex);
                throw;
            }
        }

        internal InstallSetupResult ExecuteStep(InstallSetupStep step, JToken instruction)
        {
            using (ApplicationContext.ProfilingLogger.TraceDuration<InstallApiController>("Executing installation step: " + step.Name, "Step completed"))
            {
                var model = instruction == null ? null : instruction.ToObject(step.StepType);
                var genericStepType = typeof(InstallSetupStep<>);
                Type[] typeArgs = { step.StepType };
                var typedStepType = genericStepType.MakeGenericType(typeArgs);
                try
                {
                    var method = typedStepType.GetMethods().Single(x => x.Name == "Execute");
                    return (InstallSetupResult)method.Invoke(step, new object[] { model });
                }
                catch (Exception ex)
                {
                    LogHelper.Error<InstallApiController>("Installation step " + step.Name + " failed.", ex);
                    throw;
                }
            }
        }

        private HttpResponseMessage Json(object jsonObject, HttpStatusCode status)
        {
            var response = Request.CreateResponse(status);
            var json = JObject.FromObject(jsonObject);
            response.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            return response;
        }

    }
}
