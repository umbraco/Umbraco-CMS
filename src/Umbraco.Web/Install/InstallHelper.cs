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
            var steps = new List<InstallSetupStep>();
            if (status == InstallStatus.NewInstall)
            {
                //The step order returned here is how they will appear on the front-end
                steps.AddRange(new InstallSetupStep[]
                {
                    new FilePermissionsStep()
                    {
                        Name = "Permissions",                        
                        ServerOrder = 0,
                    },
                    new UserStep(umbracoContext.Application)
                    {
                        Name = "User",
                        ServerOrder = 2,
                    },
                    new DatabaseConfigureStep(umbracoContext.Application)
                    {
                        Name = "Database",
                        ServerOrder = 1,
                    },
                    new StarterKitDownloadStep()
                    {
                        Name = "StarterKitDownload",
                        ServerOrder = 3,
                    },
                    new StarterKitInstallStep(umbracoContext.Application, umbracoContext.HttpContext)
                    {
                        Name = "StarterKitInstall",
                        ServerOrder = 4,
                    },
                    new StarterKitCleanupStep()
                    {
                        Name = "StarterKitCleanup",
                        ServerOrder = 5,
                    }
                });
                return steps;
            }
            else
            {
                //TODO: Add steps for upgrades
            }
            return null;
        }

        public static bool IsNewInstall
        {
            get
            {
                var databaseSettings = ConfigurationManager.ConnectionStrings[GlobalSettings.UmbracoConnectionName];
                if (databaseSettings != null && (
                    databaseSettings.ConnectionString.Trim() == string.Empty
                    && databaseSettings.ProviderName.Trim() == string.Empty
                    && GlobalSettings.ConfigurationStatus == string.Empty))
                {
                    return true;
                }

                return false;
            }
        }

        //private static readonly InstallerStepCollection Steps = new InstallerStepCollection
        //    {
        //        new Steps.Welcome(),
        //        new Steps.License(),
        //        new Steps.FilePermissions(),
        //        new Steps.MajorUpgradeReport(),
        //        new Steps.Database(),
        //        new Steps.DefaultUser(),
        //        //new Steps.RenderingEngine(),
        //        new Steps.Skinning(),
        //        new Steps.WebPi(),
        //        new Steps.TheEnd()
        //    };

        //internal static InstallerStepCollection InstallerSteps
        //{
        //    get { return Steps; }
        //}

        //public static void RedirectToNextStep(Page page, string currentStep)
        //{
        //    var s = InstallerSteps.GotoNextStep(currentStep);
        //    page.Response.Redirect("?installStep=" + s.Alias);
        //}

        //public static void RedirectToLastStep(Page page)
        //{
        //    var s = InstallerSteps.Get("theend");
        //    page.Response.Redirect("?installStep=" + s.Alias);
        //}


        //private static int _percentage = -1;
        //public static int Percentage 
        //{ 
        //    get { return _percentage; } 
        //    set { _percentage = value; } 
        //}

        //public static string Description { get; set; }
        //public static string Error { get; set; }


        //public static void ClearProgress()
        //{
        //    Percentage = -1;
        //    Description = string.Empty;
        //    Error = string.Empty;
        //}

        //public static void SetProgress(int percent, string description, string error)
        //{
        //    if (percent > 0)
        //        Percentage = percent;

        //    Description = description;
        //    Error = error;
        //}

        //public static string GetProgress()
        //{
        //    var pr = new ProgressResult(Percentage, Description, Error);
        //    var js = new JavaScriptSerializer();
        //    return js.Serialize(pr);
        //}
    }
}