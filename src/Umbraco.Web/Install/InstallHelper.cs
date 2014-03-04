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
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    internal class InstallHelper
    {
        private readonly UmbracoContext _umbContext;
        private readonly InstallStatusType _status;

        internal InstallHelper(UmbracoContext umbContext)
            : this(umbContext, GlobalSettings.ConfigurationStatus.IsNullOrWhiteSpace()
                ? InstallStatusType.NewInstall
                : InstallStatusType.Upgrade)
        {
        }

        internal InstallHelper(UmbracoContext umbContext, InstallStatusType status)
        {
            _umbContext = umbContext;
            _status = status;
        }

        public InstallStatusType GetStatus()
        {
            return _status;
        }

        /// <summary>
        /// Get the installer steps
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// The step order returned here is how they will appear on the front-end
        /// </remarks>
        public IEnumerable<InstallSetupStep> GetSteps()
        {
            return new List<InstallSetupStep>
            {
                new FilePermissionsStep(),
                new UserStep(_umbContext.Application, _status),
                new DatabaseConfigureStep(_umbContext.Application),
                new DatabaseInstallStep(_umbContext.Application),
                new DatabaseUpgradeStep(_umbContext.Application, _status),
                new StarterKitDownloadStep(_status),
                new StarterKitInstallStep(_status, _umbContext.Application, _umbContext.HttpContext),
                new StarterKitCleanupStep(_status),
                new SetUmbracoVersionStep(_umbContext.Application, _umbContext.HttpContext),
            };
        }
        
        //public static bool IsNewInstall
        //{
        //    get
        //    {
        //        var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];
        //        if (databaseSettings != null && (
        //            databaseSettings.ConnectionString.Trim() == string.Empty
        //            && databaseSettings.ProviderName.Trim() == string.Empty
        //            && GlobalSettings.ConfigurationStatus == string.Empty))
        //        {
        //            return true;
        //        }

        //        return false;
        //    }
        //}
    }
}