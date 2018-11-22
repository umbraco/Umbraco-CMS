using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall, 
        "StarterKitCleanup", 32, "Almost done")]
    internal class StarterKitCleanupStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _applicationContext;

        public StarterKitCleanupStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public override InstallSetupResult Execute(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            var previousStep = installSteps.Single(x => x.Name == "StarterKitDownload");
            var manifestId = Convert.ToInt32(previousStep.AdditionalData["manifestId"]);
            var packageFile = (string)previousStep.AdditionalData["packageFile"];

            CleanupInstallation(manifestId, packageFile);
            
            return null;
        }

        private void CleanupInstallation(int manifestId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);
            var installer = new Installer();
            installer.LoadConfig(packageFile);
            installer.InstallCleanUp(manifestId, packageFile);

            library.RefreshContent();
        }

        public override bool RequiresExecution(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            //this step relies on the preious one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Name == "StarterKitDownload" && x.AdditionalData.ContainsKey("manifestId")) == false)
            {
                return false;
            }

            return true;
        }
    }
}