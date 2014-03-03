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
            var status = new InstallSetup()
            {
                Status = GlobalSettings.ConfigurationStatus.IsNullOrWhiteSpace()
                    ? InstallStatus.NewInstall
                    : InstallStatus.Upgrade
            };

            //TODO: Check for user/site token

            var steps = new List<InstallSetupStep>();

            if (status.Status == InstallStatus.NewInstall)
            {
                //The step order returned here is how they will appear on the front-end
                steps.AddRange(InstallHelper.GetSteps(UmbracoContext, status.Status));
                status.Steps = steps;

                //TODO: In case someone presses f5 during install, we will attempt to resume, in order to do this??
            }
            else
            {
                //TODO: Add steps for upgrades
            }

            return status;
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

            List<Package> retval = new List<Package>();

            foreach (var package in modules)
                retval.Add(new Package() { Id = package.RepoGuid, Name = package.Text, Thumbnail = package.Thumbnail });

            return retval;
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
                    if (step.View.IsNullOrWhiteSpace() == false)
                    {
                        //Since this is a UI instruction, we will extract the model from it
                        if (instructions.Any(x => x.Key == step.Name) == false)
                        {
                            return Request.CreateValidationErrorResponse("No instruction defined for step: " + step.Name);
                        }
                        instruction = instructions[step.Name];   
                    }
                    
                    try
                    {
                        var setupData = ExecuteStep(step, instruction);

                        //update the status
                        InstallStatusTracker.SetComplete(step.Name, setupData);

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

        internal IDictionary<string, object> ExecuteStep(InstallSetupStep step, JToken instruction)
        {
            var model = instruction == null ? null : instruction.ToObject(step.StepType);
            var genericStepType = typeof(InstallSetupStep<>);
            Type[] typeArgs = { step.StepType };
            var typedStepType = genericStepType.MakeGenericType(typeArgs);
            try
            {
                var method = typedStepType.GetMethods().Single(x => x.Name == "Execute");
                return (IDictionary<string, object>)method.Invoke(step, new object[] { model });
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
