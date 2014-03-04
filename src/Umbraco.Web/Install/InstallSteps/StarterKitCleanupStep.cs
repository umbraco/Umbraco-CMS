using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("StarterKitCleanup", 8, "Cleaning up temporary files")]
    internal class StarterKitCleanupStep : InstallSetupStep<object>
    {
        private readonly InstallStatusType _status;

        public StarterKitCleanupStep(InstallStatusType status)
        {
            _status = status;
        }

        public override InstallSetupResult Execute(object model)
        {
            if (_status != InstallStatusType.NewInstall) return null;

            var installSteps = InstallStatusTracker.GetStatus();
            //this step relies on the preious one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Key == "StarterKitDownload") == false)
            {
                throw new InvalidOperationException("Could not find previous step: StarterKitDownload of the installation, package install cannot continue");
            }
            var previousStep = installSteps["StarterKitDownload"];
            var manifestId = Convert.ToInt32(previousStep.AdditionalData["manifestId"]);
            var packageFile = (string)previousStep.AdditionalData["packageFile"];

            CleanupInstallation(manifestId, packageFile);
            
            return null;
        }

        private void CleanupInstallation(int manifestId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);
            var installer = new global::umbraco.cms.businesslogic.packager.Installer();
            installer.LoadConfig(packageFile);
            installer.InstallCleanUp(manifestId, packageFile);

            library.RefreshContent();
        }

        public override bool RequiresExecution()
        {
            return _status == InstallStatusType.NewInstall;
        }
    }
}