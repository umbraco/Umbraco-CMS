using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Semver;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;

namespace Umbraco.Core.Services
{
    public interface IPackagingService : IService
    {
        #region Package Installation

        /// <summary>
        /// Returns a <see cref="CompiledPackage"/> result from an umbraco package file (zip)
        /// </summary>
        /// <param name="packageFile"></param>
        /// <returns></returns>
        CompiledPackage GetCompiledPackageInfo(FileInfo packageFile);

        /// <summary>
        /// Installs the package files contained in an umbraco package file (zip)
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="packageFile"></param>
        /// <param name="userId"></param>
        IEnumerable<string> InstallCompiledPackageFiles(PackageDefinition packageDefinition, FileInfo packageFile, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Installs the data, entities, objects contained in an umbraco package file (zip)
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="packageFile"></param>
        /// <param name="userId"></param>
        InstallationSummary InstallCompiledPackageData(PackageDefinition packageDefinition, FileInfo packageFile, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Uninstalls all versions of the package by name
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        UninstallationSummary UninstallPackage(string packageName, int userId = Constants.Security.SuperUserId);

        #endregion

        #region Installed Packages

        IEnumerable<PackageDefinition> GetAllInstalledPackages();

        /// <summary>
        /// Returns the <see cref="PackageDefinition"/> for the installation id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        PackageDefinition GetInstalledPackageById(int id);

        /// <summary>
        /// Returns all <see cref="PackageDefinition"/> for the package by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>
        /// A list of all package definitions installed for this package (i.e. original install and any upgrades)
        /// </returns>
        IEnumerable<PackageDefinition> GetInstalledPackageByName(string name);

        /// <summary>
        /// Returns a <see cref="PackageInstallType"/> for a given package name and version
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="packageVersion"></param>
        /// <param name="alreadyInstalled">If the package is an upgrade, the original/current PackageDefinition is returned</param>
        /// <returns></returns>
        PackageInstallType GetPackageInstallType(string packageName, SemVersion packageVersion, out PackageDefinition alreadyInstalled);
        void DeleteInstalledPackage(int packageId, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Persists a package definition to storage
        /// </summary>
        /// <returns></returns>
        bool SaveInstalledPackage(PackageDefinition definition);

        #endregion

        #region Created Packages

        IEnumerable<PackageDefinition> GetAllCreatedPackages();
        PackageDefinition GetCreatedPackageById(int id);
        void DeleteCreatedPackage(int id, int userId = Constants.Security.SuperUserId);

        /// <summary>
        /// Persists a package definition to storage
        /// </summary>
        /// <returns></returns>
        bool SaveCreatedPackage(PackageDefinition definition);

        /// <summary>
        /// Creates the package file and returns it's physical path
        /// </summary>
        /// <param name="definition"></param>
        string ExportCreatedPackage(PackageDefinition definition);

        #endregion

        /// <summary>
        /// This will fetch an Umbraco package file from the package repository and return the file name of the downloaded package
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="umbracoVersion"></param>
        /// <param name="userId">The current user id performing the operation</param>
        /// <returns>
        /// The file name of the downloaded package which will exist in ~/App_Data/packages
        /// </returns>
        Task<FileInfo> FetchPackageFileAsync(Guid packageId, Version umbracoVersion, int userId);
    }
}
