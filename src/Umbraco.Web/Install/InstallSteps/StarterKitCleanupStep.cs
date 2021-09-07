using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.IO;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitCleanup", 32, "Almost done")]
    internal class StarterKitCleanupStep : InstallSetupStep<object>
    {
        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            var previousStep = InstallStatusTracker.GetRequiredStep("StarterKitDownload");
            var packageId = Convert.ToInt32(previousStep.AdditionalData["packageId"]);
            var packageFile = (string)previousStep.AdditionalData["packageFile"];

            CleanupInstallation(packageId, packageFile);

            return Task.FromResult<InstallSetupResult>(null);
        }

        private void CleanupInstallation(int packageId, string packageFile)
        {
            var zipFile = new FileInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.Packages), HttpUtility.UrlDecode(packageFile)));

            if (zipFile.Exists)
                zipFile.Delete();
        }

        public override bool RequiresExecution(object model)
        {
            var starterKitDownloadStep = InstallStatusTracker.GetStep("StarterKitDownload");
            //this step relies on the previous one completed - because it has stored some information we need
            if (starterKitDownloadStep == null || !starterKitDownloadStep.AdditionalData.ContainsKey("packageId"))
            {
                return false;
            }

            return true;
        }
    }
}
