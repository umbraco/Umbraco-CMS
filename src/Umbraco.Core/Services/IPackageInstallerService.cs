using Umbraco.Core.Packaging;

namespace Umbraco.Core.Services
{
    public interface IPackageInstallerService : IService
    {
        PackageInstallationSummary InstallPackageFile(string packageFilePath, int userId);
        PackageMetaData GetMetaData(string packageFilePath);
        PackageImportIssues FindPackageImportIssues(string packageFilePath);
    }
}
