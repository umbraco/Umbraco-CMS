using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models.Packaging;

namespace Umbraco.Core.Packaging
{
    public interface IPackageInstallation
    {
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
        /// <param name="packageFileName"></param>
        /// <returns></returns>
        CompiledPackage ReadPackage(string packageFileName);
    }
}
