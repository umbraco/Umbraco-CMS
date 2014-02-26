using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Configuration;
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

        /// <summary>
        /// Gets the install setup
        /// </summary>
        /// <returns></returns>
        public InstallSetup GetSetup()
        {
            var status = new InstallSetup()
            {
                Status = GlobalSettings.ConfigurationStatus.IsNullOrWhiteSpace() ? InstallStatus.NewInstall : InstallStatus.Upgrade
            };

            //TODO: Check for user/site token

            var steps = new List<InstallStep>();

            if (status.Status == InstallStatus.NewInstall)
            {
                steps.AddRange(new[]
                {
                    new InstallStep()
                    {
                        Name = "User",
                        View = "user"
                    },
                    new InstallStep()
                    {
                        Name = "Database",
                        View = "database"
                    },
                    new InstallStep()
                    {
                        Name = "StarterKit",
                        View = "starterKit"
                    },
                });
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

        /// <summary>
        /// Does the install
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage PostPerformInstall(InstallInstructions model)
        {
            var steps = GetSetup();

            InstallStatusTracker.Initialize(steps.Steps);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the current install status
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, bool> GetStatus()
        {
            return InstallStatusTracker.GetStatus();
        } 
    }
}
