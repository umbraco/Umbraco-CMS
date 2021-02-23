using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration;
using Umbraco.Core.Security;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitDownload", "starterKit", 30, "Adding a simple website to Umbraco, will make it easier for you to get started",
        PerformsAppRestart = true)]
    internal class StarterKitDownloadStep : InstallSetupStep<Guid?>
    {
        private readonly InstallHelper _installHelper;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly IContentService _contentService;
        private readonly IPackagingService _packageService;

        public StarterKitDownloadStep(IContentService contentService, IPackagingService packageService, InstallHelper installHelper, IBackOfficeSecurityAccessor backofficeSecurityAccessor, IUmbracoVersion umbracoVersion, IUmbracoApplicationLifetime umbracoApplicationLifetime)
        {
            _installHelper = installHelper;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _umbracoVersion = umbracoVersion;
            _umbracoApplicationLifetime = umbracoApplicationLifetime;
            _contentService = contentService;
            _packageService = packageService;
        }

        //private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public override async Task<InstallSetupResult> ExecuteAsync(Guid? starterKitId)
        {
            //if there is no value assigned then use the default starter kit
            if (starterKitId.HasValue == false)
            {
                var starterKits = _installHelper.GetStarterKits().FirstOrDefault();
                if (starterKits != null)
                    starterKitId = starterKits.Id;
                else
                    return null;
            }
            else if (starterKitId.Value == Guid.Empty)
            {
                //if the startkit id is an empty GUID then it means the user has decided not to install one
                // so we'll just exit
                return null;
            }

            var (packageFile, packageId) = await DownloadPackageFilesAsync(starterKitId.Value);

            _umbracoApplicationLifetime.Restart();

            return new InstallSetupResult(new Dictionary<string, object>
            {
                {"packageId", packageId},
                {"packageFile", packageFile}
            });
        }

        private async Task<(string packageFile, int packageId)> DownloadPackageFilesAsync(Guid kitGuid)
        {
            //Go get the package file from the package repo
            var packageFile = await _packageService.FetchPackageFileAsync(kitGuid, _umbracoVersion.Version, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
            if (packageFile == null) throw new InvalidOperationException("Could not fetch package file " + kitGuid);

            //add an entry to the installedPackages.config
            var compiledPackage = _packageService.GetCompiledPackageInfo(packageFile);
            var packageDefinition = PackageDefinition.FromCompiledPackage(compiledPackage);
            packageDefinition.PackagePath = packageFile.FullName;

            _packageService.SaveInstalledPackage(packageDefinition);

            _packageService.InstallCompiledPackageFiles(packageDefinition, packageFile, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(-1));

            return (compiledPackage.PackageFile.Name, packageDefinition.Id);
        }

        /// <summary>
        /// Don't show the view if there's already packages installed
        /// </summary>
        public override string View => _packageService.GetAllInstalledPackages().Any() ? string.Empty : base.View;

        public override bool RequiresExecution(Guid? model)
        {
            //Don't execute if it's an empty GUID - meaning the user has chosen not to install one
            if (model.HasValue && model.Value == Guid.Empty)
            {
                return false;
            }

            //Don't continue if there's already packages installed
            if (_packageService.GetAllInstalledPackages().Any())
                return false;

            if (_contentService.GetRootContent().Any())
                return false;

            return true;
        }
    }
}
