using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.packager;
using Umbraco.Core;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall,
        "StarterKitDownload", "starterKit", 30, "Downloading a starter website from our.umbraco.org, hold tight, this could take a little while")]
    internal class StarterKitDownloadStep : InstallSetupStep<Guid?>
    {
        private readonly ApplicationContext _applicationContext;

        public StarterKitDownloadStep(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public override InstallSetupResult Execute(Guid? starterKitId)
        {
            //if there is no value assigned then use the default starter kit
            if (starterKitId.HasValue == false)
            {
                //get a default package GUID (currently the first one found)
                var r = new org.umbraco.our.Repository();
                var modules = r.Modules();
                var defaultPackageId = modules.First().RepoGuid;
                starterKitId = defaultPackageId;
            }

            var result = DownloadPackageFiles(starterKitId.Value);

            return new InstallSetupResult(new Dictionary<string, object>
            {
                {"manifestId", result.Item2},
                {"packageFile", result.Item1}
            });
        }

        private Tuple<string, int> DownloadPackageFiles(Guid kitGuid)
        {
            var repo = global::umbraco.cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
            if (repo == null)
            {
                throw new InvalidOperationException("No repository found with id " + RepoGuid);
            }
            if (repo.HasConnection() == false)
            {
                throw new InvalidOperationException("Cannot connect to repository");                
            }
            var installer = new Installer();

            var tempFile = installer.Import(repo.fetch(kitGuid.ToString()));
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

        public override bool RequiresExecution()
        {
            if (InstalledPackage.GetAllInstalledPackages().Count > 0)
                return false;

            if (_applicationContext.Services.ContentService.GetRootContent().Any())
                return false;

            return true;
        }
    }
}