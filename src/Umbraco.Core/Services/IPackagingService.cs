using System.Xml.Linq;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPackagingService : IService
{
    /// <summary>
    ///     Returns a <see cref="CompiledPackage" /> result from an umbraco package file (zip)
    /// </summary>
    /// <param name="packageXml"></param>
    /// <returns></returns>
    CompiledPackage GetCompiledPackageInfo(XDocument packageXml);

    /// <summary>
    ///     Installs the data, entities, objects contained in an umbraco package file (zip)
    /// </summary>
    /// <param name="packageXmlFile"></param>
    /// <param name="userId"></param>
    InstallationSummary InstallCompiledPackageData(FileInfo packageXmlFile, int userId = Constants.Security.SuperUserId);

    InstallationSummary InstallCompiledPackageData(XDocument? packageXml, int userId = Constants.Security.SuperUserId);

    /// <summary>
    ///     Returns the advertised installed packages
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<InstalledPackage>> GetAllInstalledPackagesAsync();

    /// <summary>
    ///     Returns installed packages collected from the package migration plans.
    /// </summary>
    Task<PagedModel<InstalledPackage>> GetInstalledPackagesFromMigrationPlansAsync(int skip, int take);

    InstalledPackage? GetInstalledPackageByName(string packageName);

    /// <summary>
    ///     Returns the created packages as a paged model.
    /// </summary>
    /// <param name="skip">The amount of items to skip.</param>
    /// <param name="take">The amount of items to take.</param>
    Task<PagedModel<PackageDefinition>> GetCreatedPackagesAsync(int skip, int take);

    /// <summary>
    ///     Returns a created package by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    PackageDefinition? GetCreatedPackageById(int id);

    /// <summary>
    ///     Returns a created package by key.
    /// </summary>
    /// <param name="key">The key of the package.</param>
    /// <returns>The package or null if the package was not found.</returns>
    Task<PackageDefinition?> GetCreatedPackageByKeyAsync(Guid key);

    /// <summary>
    ///     Deletes a created package by key.
    /// </summary>
    /// <param name="key">The key of the package.</param>
    /// <param name="userKey">Key of the user deleting the package.</param>
    Task<Attempt<PackageDefinition?, PackageOperationStatus>> DeleteCreatedPackageAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Creates a new package.
    /// </summary>
    /// <param name="package"><see cref="PackageDefinition" />model for the package to create.</param>
    /// <param name="userKey">Key of the user performing the create.</param>
    Task<Attempt<PackageDefinition, PackageOperationStatus>> CreateCreatedPackageAsync(PackageDefinition package, Guid userKey);

    /// <summary>
    ///     Updates a created package.
    /// </summary>
    /// <param name="package"><see cref="PackageDefinition" />model for the package to update.</param>
    /// <param name="userKey">Key of the user performing the update.</param>
    Task<Attempt<PackageDefinition, PackageOperationStatus>> UpdateCreatedPackageAsync(PackageDefinition package, Guid userKey);

    /// <summary>
    ///     Creates the package file and returns it's physical path
    /// </summary>
    /// <param name="definition"></param>
    string ExportCreatedPackage(PackageDefinition definition);

    /// <summary>
    ///     Gets the package file stream.
    /// </summary>
    /// <param name="definition"></param>
    Stream? GetPackageFileStream(PackageDefinition definition) => null;
}
