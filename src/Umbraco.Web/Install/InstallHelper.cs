using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;

using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    internal class InstallHelper
    {
        private readonly UmbracoContext _umbContext;
        private InstallationType? _installationType;

        internal InstallHelper(UmbracoContext umbContext)
        {
            _umbContext = umbContext;            
        }


        /// <summary>
        /// Get the installer steps
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The step order returned here is how they will appear on the front-end if they have views assigned
        /// </remarks>
        public IEnumerable<InstallSetupStep> GetAllSteps()
        {
            return new List<InstallSetupStep>
            {                
                new UserStep(_umbContext.Application),
                new Upgrade(),
                new FilePermissionsStep(),
                new MajorVersion7UpgradeReport(_umbContext.Application),
                new DatabaseConfigureStep(_umbContext.Application),
                new DatabaseInstallStep(_umbContext.Application),
                new DatabaseUpgradeStep(_umbContext.Application),
                new StarterKitDownloadStep(_umbContext.Application),
                new StarterKitInstallStep(_umbContext.Application, _umbContext.HttpContext),
                new StarterKitCleanupStep(_umbContext.Application),
                new SetUmbracoVersionStep(_umbContext.Application, _umbContext.HttpContext),
            };
        }

        /// <summary>
        /// Returns the steps that are used only for the current installation type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstallSetupStep> GetStepsForCurrentInstallType()
        {
            return GetAllSteps().Where(x => x.InstallTypeTarget.HasFlag(GetInstallationType()));
        }

        public InstallationType GetInstallationType()
        {
            return _installationType ?? (_installationType = IsNewInstall ? InstallationType.NewInstall : InstallationType.Upgrade).Value;
        }

        /// <summary>
        /// Checks if this is a brand new install meaning that there is no configured version and there is no configured database connection
        /// </summary>
        private bool IsNewInstall
        {
            get
            {
                var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];
                if (GlobalSettings.ConfigurationStatus.IsNullOrWhiteSpace()
                    && _umbContext.Application.DatabaseContext.IsConnectionStringConfigured(databaseSettings) == false)
                {
                    //no version or conn string configured, must be a brand new install
                    return true;
                }

                return false;
            }
        }
    }
}