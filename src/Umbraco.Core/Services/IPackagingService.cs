using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Core.Services;

public interface IPackagingService : IService
{
    /// <summary>
    ///     Returns a <see cref="CompiledPackage" /> result from an umbraco package file (zip)
    /// </summary>
    /// <param name="packageFile"></param>
    /// <returns></returns>
    CompiledPackage GetCompiledPackageInfo(XDocument packageXml);

    /// <summary>
    ///     Installs the data, entities, objects contained in an umbraco package file (zip)
    /// </summary>
    /// <param name="packageFile"></param>
    /// <param name="userId"></param>
    InstallationSummary InstallCompiledPackageData(FileInfo packageXmlFile, int userId = Constants.Security.SuperUserId);

    InstallationSummary InstallCompiledPackageData(XDocument? packageXml, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Returns the advertised installed packages
    /// </summary>
    /// <returns></returns>
    IEnumerable<InstalledPackage> GetAllInstalledPackages();

    InstalledPackage? GetInstalledPackageByName(string packageName);

    /// <summary>
    ///     Returns the created packages
    /// </summary>
    /// <returns></returns>
    IEnumerable<PackageDefinition?> GetAllCreatedPackages();

    /// <summary>
    ///     Returns a created package by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    PackageDefinition? GetCreatedPackageById(int id);

    void DeleteCreatedPackage(int id, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Persists a package definition to storage
    /// </summary>
    /// <returns></returns>
    bool SaveCreatedPackage(PackageDefinition definition);

    /// <summary>
    ///     Creates the package file and returns it's physical path
    /// </summary>
    /// <param name="definition"></param>
    string ExportCreatedPackage(PackageDefinition definition);
}
