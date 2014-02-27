using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install.InstallSteps;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install
{
    internal static class InstallHelper
    {

        public static IEnumerable<InstallSetupStep> GetSteps(
            UmbracoContext umbracoContext,
            InstallStatus status)
        {
            //TODO: Add UserToken step to save our user token with Mother

            var steps = new List<InstallSetupStep>(new InstallSetupStep[]
            {
                new FilePermissionsStep()
                {
                    ServerOrder = 0,
                },
                new UserStep(umbracoContext.Application, status)
                {
                    ServerOrder = 4,
                },
                new DatabaseConfigureStep(umbracoContext.Application)
                {
                    ServerOrder = 1,
                },
                new DatabaseInstallStep(umbracoContext.Application)
                {
                    ServerOrder = 2,
                },
                new DatabaseUpgradeStep(umbracoContext.Application)
                {
                    ServerOrder = 3,
                },
                new StarterKitDownloadStep(status)
                {
                    ServerOrder = 5,
                },
                new StarterKitInstallStep(status, umbracoContext.Application, umbracoContext.HttpContext)
                {
                    ServerOrder = 6,
                },
                new StarterKitCleanupStep(status)
                {
                    ServerOrder = 7,
                },
                new SetUmbracoVersionStep(umbracoContext.Application, umbracoContext.HttpContext)
                {
                    ServerOrder = 8
                }
            });

            return steps;
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