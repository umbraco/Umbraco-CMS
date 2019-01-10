using System.Xml.Linq;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageInstallation
    {
        //fixme: The reason why this isn't used currently is because package installation needs to be done in phases since
        // there are app domain reboots involved so a single method cannot be used. This needs to either be split into several
        // methods or return an object with a callback to proceed to the next step.
        InstallationSummary InstallPackage(string packageFilePath, int userId);
        MetaData GetMetaData(string packageFilePath);
        PreInstallWarnings GetPreInstallWarnings(string packageFilePath);
        XElement GetConfigXmlElement(string packageFilePath);
    }
}
