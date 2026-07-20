using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging;

/// <summary>
///     Defines methods for installing Umbraco packages.
/// </summary>
public interface IPackageInstallation
{
    /// <summary>
    ///     Installs a package's data and entities.
    /// </summary>
    /// <param name="compiledPackage">The compiled package containing the data to install.</param>
    /// <param name="userId">The identifier of the user performing the installation.</param>
    /// <param name="packageDefinition">When this method returns, contains the package definition created during installation.</param>
    /// <returns>An <see cref="InstallationSummary"/> containing details about what was installed.</returns>
    /// <remarks>
    ///     The resulting <see cref="PackageDefinition"/> is only if we wanted to persist what was saved during package data installation.
    ///     This used to be for the installedPackages.config but we don't have that anymore and don't really want it if we can help it.
    ///     Possibly, we could continue to persist that file so that you could uninstall package data for an installed package in the
    ///     back office (but it won't actually uninstall the package until you do that via NuGet). If we want that functionality we'll have
    ///     to restore a bunch of deleted code.
    /// </remarks>
    InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId, out PackageDefinition packageDefinition);

    /// <summary>
    ///     Reads the package XML and returns the <see cref="CompiledPackage" /> model.
    /// </summary>
    /// <param name="packageXmlFile">The XML document containing the package definition.</param>
    /// <returns>A <see cref="CompiledPackage"/> model representing the package.</returns>
    CompiledPackage ReadPackage(XDocument? packageXmlFile);
}
