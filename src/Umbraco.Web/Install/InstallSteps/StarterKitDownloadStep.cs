using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install.Models;
using Umbraco.Web.Security;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitDownload", "starterKit", 30, "Adding a simple website to Umbraco, will make it easier for you to get started",
        PerformsAppRestart = true)]
    internal class StarterKitDownloadStep : InstallSetupStep<Guid?>
    {
        private readonly ApplicationContext _applicationContext;
        private readonly WebSecurity _security;
        private readonly HttpContextBase _httpContext;

        public StarterKitDownloadStep(ApplicationContext applicationContext, WebSecurity security, HttpContextBase httpContext)
        {
            _applicationContext = applicationContext;
            _security = security;
            _httpContext = httpContext;
        }

        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public override InstallSetupResult Execute(Guid? starterKitId)
        {
            //if there is no value assigned then use the default starter kit
            if (starterKitId.HasValue == false)
            {
                var installHelper = new InstallHelper(UmbracoContext.Current);
                var starterKits = installHelper.GetStarterKits().FirstOrDefault();
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

            var result = DownloadPackageFiles(starterKitId.Value);

            _applicationContext.RestartApplicationPool(_httpContext);

            return new InstallSetupResult(new Dictionary<string, object>
            {
                {"manifestId", result.Item2},
                {"packageFile", result.Item1}
            });
        }

        private Tuple<string, int> DownloadPackageFiles(Guid kitGuid)
        {          
            var installer = new Installer();

            //Go get the package file from the package repo
            var packageFile = _applicationContext.Services.PackagingService.FetchPackageFile(kitGuid, UmbracoVersion.Current, _security.GetUserId());

            var tempFile = installer.Import(packageFile);
            installer.LoadConfig(tempFile);
            var pId = installer.CreateManifest(tempFile, kitGuid.ToString(), RepoGuid);

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

        public override string View
        {
            get { return (InstalledPackage.GetAllInstalledPackages().Count > 0) ? string.Empty : base.View; }
        }

        public override bool RequiresExecution(Guid? model)
        {
            //Don't execute if it's an empty GUID - meaning the user has chosen not to install one
            if (model.HasValue && model.Value == Guid.Empty)
            {
                return false;
            }

            if (InstalledPackage.GetAllInstalledPackages().Count > 0)
                return false;

            if (_applicationContext.Services.ContentService.GetRootContent().Any())
                return false;

            return true;
        }
    }
}