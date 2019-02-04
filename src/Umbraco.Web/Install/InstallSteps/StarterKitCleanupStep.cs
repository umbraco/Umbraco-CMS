using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitCleanup", 32, "Almost done")]
    internal class StarterKitCleanupStep : InstallSetupStep<object>
    {
        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            var previousStep = installSteps.Single(x => x.Name == "StarterKitDownload");
            var packageId = Convert.ToInt32(previousStep.AdditionalData["packageId"]);
            var packageFile = (string)previousStep.AdditionalData["packageFile"];

            CleanupInstallation(packageId, packageFile);

            return Task.FromResult<InstallSetupResult>(null);
        }

        private void CleanupInstallation(int packageId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);

            // TODO: When does the zip file get deleted?
        }

        public override bool RequiresExecution(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            //this step relies on the previous one completed - because it has stored some information we need
            if (installSteps.Any(x => x.Name == "StarterKitDownload" && x.AdditionalData.ContainsKey("packageId")) == false)
            {
                return false;
            }

            return true;
        }
    }
}
