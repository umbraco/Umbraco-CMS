using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageInstallation : IService
    {
        InstallationSummary InstallPackage(string packageFilePath, int userId);
        MetaData GetMetaData(string packageFilePath);
        PreInstallWarnings GetPreInstallWarnings(string packageFilePath);
    }
}