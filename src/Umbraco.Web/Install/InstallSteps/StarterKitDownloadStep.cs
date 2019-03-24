using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Packaging;
using Umbraco.Web.Composing;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitDownload", "starterKit", 30, "Adding a simple website to Umbraco, will make it easier for you to get started",
        PerformsAppRestart = true)]
    internal class StarterKitDownloadStep : InstallSetupStep<Guid?>
    {
        private readonly InstallHelper _installHelper;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;
        private readonly IPackagingService _packageService;

        public StarterKitDownloadStep(IContentService contentService, IPackagingService packageService, InstallHelper installHelper, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _installHelper = installHelper;
            _umbracoContextAccessor = umbracoContextAccessor;
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

            UmbracoApplication.Restart();

            return new InstallSetupResult(new Dictionary<string, object>
            {
                {"packageId", packageId},
                {"packageFile", packageFile}
            });
        }

        private async Task<(string packageFile, int packageId)> DownloadPackageFilesAsync(Guid kitGuid)
        {
            //Go get the package file from the package repo
            var packageFile = await _packageService.FetchPackageFileAsync(kitGuid, UmbracoVersion.Current, _umbracoContextAccessor.UmbracoContext.Security.GetUserId().ResultOr(0));
            if (packageFile == null) throw new InvalidOperationException("Could not fetch package file " + kitGuid);

            //add an entry to the installedPackages.config
            var compiledPackage = _packageService.GetCompiledPackageInfo(packageFile);
            var packageDefinition = PackageDefinition.FromCompiledPackage(compiledPackage);
            packageDefinition.PackagePath = packageFile.FullName;

            _packageService.SaveInstalledPackage(packageDefinition);

            _packageService.InstallCompiledPackageFiles(packageDefinition, packageFile, _umbracoContextAccessor.UmbracoContext.Security.GetUserId().ResultOr(-1));

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
