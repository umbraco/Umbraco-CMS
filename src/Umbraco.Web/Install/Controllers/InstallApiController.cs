using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        /// <summary>
        /// Gets the install setup
        /// </summary>
        /// <returns></returns>
        public InstallSetup GetSetup()
        {
            var helper = new InstallHelper(UmbracoContext);

            var setup = new InstallSetup()
            {
                Status = helper.GetStatus()
            };

            //TODO: Check for user/site token

            var steps = new List<InstallSetupStep>();

            steps.AddRange(helper.GetSteps());
            setup.Steps = steps;

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

        /// <summary>
        /// Does the install
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PostPerformInstall(IDictionary<string, JToken> instructions)
        {
            if (instructions == null) throw new ArgumentNullException("instructions");

            var steps = GetSetup().Steps.OrderBy(x => x.ServerOrder).ToArray();

            var status = InstallStatusTracker.GetStatus();
            if (status.Count == 0)
            {
                status = InstallStatusTracker.Initialize(steps);
            }

            foreach (var step in steps)
            {
                var step1 = step;
                var stepStatus = status.Single(x => x.Key == step1.Name);
                //if it is not complete, then we need to execute it
                if (stepStatus.Value.IsComplete == false)
                {
                    JToken instruction = null;
                    if (step.HasUIElement)
                    {
                        //Since this is a UI instruction, we will extract the model from it
                        if (instructions.Any(x => x.Key == step.Name) == false)
                        {
                            return Request.CreateValidationErrorResponse("No instruction defined for step: " + step.Name);
                        }
                        instruction = instructions[step.Name];   
                    }

                    //If this step doesn't require execution then continue to the next one.
                    if (step.RequiresExecution() == false)
                    {
                        continue;
                    }

                    try
                    {
                        var setupData = ExecuteStep(step, instruction);

                        //update the status
                        InstallStatusTracker.SetComplete(step.Name, setupData.SavedStepData);
                        return Json(new
                        {
                            complete = false,
                            stepCompleted = step.Name
                        }, HttpStatusCode.OK);
                    }
                    catch (InstallException iex)
                    {
                        return Json(iex.Result, HttpStatusCode.BadRequest);
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateValidationErrorResponse("An error occurred executing the step: " + step.Name + ". Error: " + ex.Message);
                    }
                }
            }

            InstallStatusTracker.Reset();

            return Json(new { complete = true }, HttpStatusCode.OK);
        }

        internal InstallSetupResult ExecuteStep(InstallSetupStep step, JToken instruction)
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
        
        private HttpResponseMessage Json(object jsonObject, HttpStatusCode status)
        {
            var response = Request.CreateResponse(status);
            var json = JObject.FromObject(jsonObject);
            response.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            return response;
        }
        
    }
}
