using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitInstall", 31, "",
        PerformsAppRestart = true)]
    internal class StarterKitInstallStep : InstallSetupStep<object>
    {
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IPackagingService _packagingService;

        public StarterKitInstallStep(IUmbracoApplicationLifetime umbracoApplicationLifetime, IBackOfficeSecurityAccessor backofficeSecurityAccessor, IPackagingService packagingService)
        {
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _packagingService = packagingService;
        }


        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            var installSteps = InstallStatusTracker.GetStatus().ToArray();
            var previousStep = installSteps.Single(x => x.Name == "StarterKitDownload");
            var packageId = Convert.ToInt32(previousStep.AdditionalData["packageId"]);

            InstallBusinessLogic(packageId);

            _umbracoApplicationLifetime.Restart();



            return Task.FromResult<InstallSetupResult>(null);
        }

        private void InstallBusinessLogic(int packageId)
        {
            var definition = _packagingService.GetInstalledPackageById(packageId);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + packageId);

            var packageFile = new FileInfo(definition.PackagePath);

            _packagingService.InstallCompiledPackageData(definition, packageFile, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(-1));
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
