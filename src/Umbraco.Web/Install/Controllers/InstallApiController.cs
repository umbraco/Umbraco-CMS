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

        /// <summary>
        /// Checks if the db can be connected to
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PostCheckDbConnection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the db credentials are correct
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PostCheckDbCredentials()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Package> GetPackages()
        {
            var r = new org.umbraco.our.Repository();
            var modules = r.Modules();

            return modules.Select(package => new Package()
            {
                Id = package.RepoGuid,
                Name = package.Text,
                Thumbnail = package.Thumbnail
            });
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

            //var queue = new Queue<InstallTrackingItem>(status);
            //while (queue.Count > 0)
            //{
            //    var stepStatus = queue.Dequeue();

            //    //if it is not complete, then we need to execute it
            //    if (stepStatus.IsComplete == false)
            //    {
            //        var step = InstallHelper.GetAllSteps().Single(x => x.Name == stepStatus.Name);

            //        JToken instruction = null;
            //        if (step.HasUIElement)
            //        {
            //            //Since this is a UI instruction, we will extract the model from it
            //            if (installModel.Instructions.Any(x => x.Key == step.Name) == false)
            //            {
            //                throw new HttpResponseException(Request.CreateValidationErrorResponse("No instruction defined for step: " + step.Name));
            //            }
            //            instruction = installModel.Instructions[step.Name];
            //        }

            //        //If this step doesn't require execution then continue to the next one.
            //        if (step.RequiresExecution() == false)
            //        {
            //            //set this as complete and continue
            //            InstallStatusTracker.SetComplete(installModel.InstallId, stepStatus.Name, null);
            //            continue;
            //        }

            //        try
            //        {
            //            var setupData = ExecuteStep(step, instruction);

            //            //update the status
            //            InstallStatusTracker.SetComplete(installModel.InstallId, step.Name, setupData != null ? setupData.SavedStepData : null);

            //            //get the next step if there is one

            //            var nextStep = "";
            //            if ((index + 1) < status.Length)
            //            {

            //                nextStep = status[index + 1].Name;
            //            }

            //            //check if there's a custom view to return for this step
            //            if (setupData != null && setupData.View.IsNullOrWhiteSpace() == false)
            //            {
            //                return new InstallProgressResultModel(false, step.Name, nextStep, setupData.View, setupData.ViewModel);
            //            }

            //            return new InstallProgressResultModel(false, step.Name, nextStep);
            //        }
            //        catch (Exception ex)
            //        {
            //            if (ex is TargetInvocationException && ex.InnerException != null)
            //            {
            //                ex = ex.InnerException;
            //            }

            //            var installException = ex as InstallException;
            //            if (installException != null)
            //            {
            //                throw new HttpResponseException(Request.CreateValidationErrorResponse(new
            //                {
            //                    view = installException.View,
            //                    model = installException.ViewModel,
            //                    message = installException.Message
            //                }));
            //            }

            //            throw new HttpResponseException(
            //                Request.CreateValidationErrorResponse("An error occurred executing the step: " + step.Name + ". Error: " + ex.Message));
            //        }
            //    }

            //}

            for (var index = 0; index < status.Length; index++)
            {
                var stepStatus = status[index];
                //if it is not complete, then we need to execute it
                if (stepStatus.IsComplete == false)
                {
                    var step = InstallHelper.GetAllSteps().Single(x => x.Name == stepStatus.Name);

                    JToken instruction = null;
                    if (step.HasUIElement)
                    {
                        //Since this is a UI instruction, we will extract the model from it
                        if (installModel.Instructions.Any(x => x.Key == step.Name) == false)
                        {
                            throw new HttpResponseException(Request.CreateValidationErrorResponse("No instruction defined for step: " + step.Name));
                        }
                        instruction = installModel.Instructions[step.Name];
                    }

                    //If this step doesn't require execution then continue to the next one.
                    if (step.RequiresExecution() == false)
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

                        //get the next step if there is one
                        var nextStep = "";
                        if ((index + 1) < status.Length)
                        {

                            nextStep = status[index + 1].Name;
                        }

                        //check if there's a custom view to return for this step
                        if (setupData != null && setupData.View.IsNullOrWhiteSpace() == false)
                        {
                            return new InstallProgressResultModel(false, step.Name, nextStep, setupData.View, setupData.ViewModel);
                        }

                        return new InstallProgressResultModel(false, step.Name, nextStep);
                    }
                    catch (Exception ex)
                    {
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

                        throw new HttpResponseException(
                            Request.CreateValidationErrorResponse("An error occurred executing the step: " + step.Name + ". Error: " + ex.Message));
                    }
                }
            }

            InstallStatusTracker.Reset();
            return new InstallProgressResultModel(true, "", "");

            //return Json(new { complete = true }, HttpStatusCode.OK);
        }

        //private InstallSetupStep IterateNextRequiredStep(Queue<InstallTrackingItem> queue)
        //{
        //    var step = InstallHelper.GetAllSteps().Single(x => x.Name == stepStatus.Name);
        //}

        internal InstallSetupResult ExecuteStep(InstallSetupStep step, JToken instruction)
        {
            using (DisposableTimer.TraceDuration<InstallApiController>("Executing installation step: " + step.Name, "Step completed"))
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
