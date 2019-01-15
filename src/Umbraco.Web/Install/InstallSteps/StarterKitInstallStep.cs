using System;
using System.Linq;
using System.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Install.Models;
using Umbraco.Web._Legacy.Packager;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitInstall", 31, "",
        PerformsAppRestart = true)]
    internal class StarterKitInstallStep : InstallSetupStep<object>
    {
        private readonly HttpContextBase _httContext;

        public StarterKitInstallStep(HttpContextBase httContext)
        {
            _httContext = httContext;
        }


        public override InstallSetupResult Execute(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            var previousStep = installSteps.Single(x => x.Name == "StarterKitDownload");
            var manifestId = Convert.ToInt32(previousStep.AdditionalData["manifestId"]);
            var packageFile = (string)previousStep.AdditionalData["packageFile"];

            InstallBusinessLogic(manifestId, packageFile);

            Current.RestartAppPool(_httContext);

            return null;
        }

        private void InstallBusinessLogic(int manifestId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);
            var installer = new Installer();
            installer.LoadConfig(packageFile);
            installer.InstallBusinessLogic(manifestId, packageFile);
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
