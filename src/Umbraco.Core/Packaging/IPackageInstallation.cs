using System.Collections.Generic;
using System.IO;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging
{
    public interface IPackageInstallation
    {
        /// <summary>
        /// This will run the uninstall sequence for this <see cref="PackageDefinition"/>
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        UninstallationSummary UninstallPackage(PackageDefinition packageDefinition, int userId);

        /// <summary>
        /// Installs a packages data and entities
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="compiledPackage"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        InstallationSummary InstallPackageData(PackageDefinition packageDefinition, CompiledPackage compiledPackage, int userId);

        /// <summary>
        /// Reads the package (xml) file and returns the <see cref="CompiledPackage"/> model
        /// </summary>
        /// <param name="packageFile"></param>
        /// <returns></returns>
        // TODO: Will be an xml structure not FileInfo
        CompiledPackage ReadPackage(FileInfo packageFile);
    }
}
