using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration;
using Umbraco.Web.Composing;
using Umbraco.Web.Install.Models;
using Umbraco.Web._Legacy.Packager;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitDownload", "starterKit", 30, "Adding a simple website to Umbraco, will make it easier for you to get started",
        PerformsAppRestart = true)]
    internal class StarterKitDownloadStep : InstallSetupStep<Guid?>
    {
        private readonly InstallHelper _installHelper;
        private readonly UmbracoContext _umbracoContext;
        private readonly IContentService _contentService;
        private readonly IPackagingService _packageService;

        public StarterKitDownloadStep(IContentService contentService, IPackagingService packageService, InstallHelper installHelper, UmbracoContext umbracoContext)
        {
            _installHelper = installHelper;
            _umbracoContext = umbracoContext;
            _contentService = contentService;
            _packageService = packageService;
        }

        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

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

            var result = await DownloadPackageFilesAsync(starterKitId.Value);

            Current.RestartAppPool();

            return new InstallSetupResult(new Dictionary<string, object>
            {
                {"manifestId", result.Item2},
                {"packageFile", result.Item1}
            });
        }

        private async Task<Tuple<string, int>> DownloadPackageFilesAsync(Guid kitGuid)
        {
            var installer = new Installer();

            //Go get the package file from the package repo
            var packageFile = await _packageService.FetchPackageFileAsync(kitGuid, UmbracoVersion.Current, _umbracoContext.Security.GetUserId().ResultOr(0));

            var tempFile = installer.Import(packageFile);
            installer.LoadConfig(tempFile);
            var pId = installer.CreateManifest(kitGuid);

            InstallPackageFiles(pId, tempFile);

            return new Tuple<string, int>(tempFile, pId);
        }

        private void InstallPackageFiles(int manifestId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);
            var installer = new Installer();
            installer.LoadConfig(packageFile);
            installer.InstallFiles(manifestId, packageFile);

        }

        public override string View => _packageService.GetAllInstalledPackages().Any() ? string.Empty : base.View;

        public override bool RequiresExecution(Guid? model)
        {
            //Don't execute if it's an empty GUID - meaning the user has chosen not to install one
            if (model.HasValue && model.Value == Guid.Empty)
            {
                return false;
            }

            if (_packageService.GetAllInstalledPackages().Any())
                return false;

            if (_contentService.GetRootContent().Any())
                return false;

            return true;
        }
    }
}
