using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Packaging;

namespace Umbraco.Cms.Core.Packaging;

public interface IPackageInstallation
{
    /// <summary>
    ///     Installs a packages data and entities
    /// </summary>
    /// <param name="packageDefinition"></param>
    /// <param name="compiledPackage"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    // TODO: The resulting PackageDefinition is only if we wanted to persist what was saved during package data installation.
    // This used to be for the installedPackages.config but we don't have that anymore and don't really want it if we can help it.
    // Possibly, we could continue to persist that file so that you could uninstall package data for an installed package in the
    // back office (but it won't actually uninstall the package until you do that via nuget). If we want that functionality we'll have
    // to restore a bunch of deleted code.
    InstallationSummary InstallPackageData(CompiledPackage compiledPackage, int userId, out PackageDefinition packageDefinition);

    /// <summary>
    ///     Reads the package xml and returns the <see cref="CompiledPackage" /> model
    /// </summary>
    /// <param name="packageFile"></param>
    /// <returns></returns>
    CompiledPackage ReadPackage(XDocument? packageXmlFile);
}
