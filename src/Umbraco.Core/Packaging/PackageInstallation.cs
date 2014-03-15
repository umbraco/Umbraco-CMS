using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageInstallation
    {
        InstallationSummary InstallPackage(string packageFilePath, int userId = 0);
        MetaData GetPackageMetaData(string packageFilePath);
    }

    internal class PackageInstallation : IPackageInstallation
    {
        private readonly PackagingService _packagingService;
        private readonly PackageExtraction _packageExtraction;

        public PackageInstallation(PackagingService packagingService, PackageExtraction packageExtraction)
        {
            _packagingService = packagingService;
            _packageExtraction = packageExtraction;
        }

        public InstallationSummary InstallPackage(string packageFilePath, int userId = 0)
        {
            var summary = new InstallationSummary();
            return summary;
        }

        public MetaData GetPackageMetaData(string packageFilePath)
        {
            var metaData = new MetaData();
            return metaData;
        }
    }
}