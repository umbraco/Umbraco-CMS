using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    public interface IPackageInstallation
    {
        /// <summary>
        /// This will run the uninstallation sequence for this <see cref="PackageDefinition"/>
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
        /// Installs a packages files
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="compiledPackage"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<string> InstallPackageFiles(PackageDefinition packageDefinition, CompiledPackage compiledPackage, int userId);

        /// <summary>
        /// Reads the package (zip) file and returns the <see cref="CompiledPackage"/> model
        /// </summary>
        /// <param name="packageFile"></param>
        /// <returns></returns>
        CompiledPackage ReadPackage(FileInfo packageFile);
    }
}
