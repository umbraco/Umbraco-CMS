using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep("StarterKitDownload", "starterKit")]
    internal class StarterKitDownloadStep : InstallSetupStep<Guid>
    {
        private readonly InstallStatus _status;

        public StarterKitDownloadStep(InstallStatus status)
        {
            _status = status;
        }

        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public override IDictionary<string, object> Execute(Guid starterKitId)
        {
            if (_status != InstallStatus.NewInstall) return null;

            var result = DownloadPackageFiles(starterKitId);

            return new Dictionary<string, object>
            {
                {"manifestId", result.Item2},
                {"packageFile", result.Item1}
            };
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
            var installer = new global::umbraco.cms.businesslogic.packager.Installer();

            var tempFile = installer.Import(repo.fetch(kitGuid.ToString()));
            installer.LoadConfig(tempFile);
            var pId = installer.CreateManifest(tempFile, kitGuid.ToString(), RepoGuid);

            InstallPackageFiles(pId, tempFile);

            return new Tuple<string, int>(tempFile, pId);
        }

        private void InstallPackageFiles(int manifestId, string packageFile)
        {
            packageFile = HttpUtility.UrlDecode(packageFile);
            var installer = new global::umbraco.cms.businesslogic.packager.Installer();
            installer.LoadConfig(packageFile);
            installer.InstallFiles(manifestId, packageFile);
            
        }

    }
}