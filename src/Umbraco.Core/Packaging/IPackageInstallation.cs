using System.Xml.Linq;
using Umbraco.Core.Packaging.Models;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageInstallation
    {
        InstallationSummary InstallPackage(string packageFilePath, int userId);
        MetaData GetMetaData(string packageFilePath);
        PreInstallWarnings GetPreInstallWarnings(string packageFilePath);
        XElement GetConfigXmlElement(string packageFilePath);
    }
}