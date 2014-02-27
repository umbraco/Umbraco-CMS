using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("StarterKitCleanup", "")]
    internal class StarterKitCleanupStep : InstallSetupStep<object>
    {
        private readonly InstallStatus _status;

        public StarterKitCleanupStep(InstallStatus status)
        {
            _status = status;
        }

        public override IDictionary<string, object> Execute(object model)
        {
            if (_status != InstallStatus.NewInstall) return null;

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

    }
}