using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install.Models;

namespace Umbraco.Cms.Core.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitCleanup", 32, "Almost done")]
    internal class StarterKitCleanupStep : InstallSetupStep<object>
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public StarterKitCleanupStep(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

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
            var zipFile = new FileInfo(Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Packages), WebUtility.UrlDecode(packageFile)));

            if (zipFile.Exists)
                zipFile.Delete();
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
