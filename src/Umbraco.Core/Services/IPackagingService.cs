using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
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
        /// <param name="packageFileName"></param>
        /// <returns></returns>
        CompiledPackage GetCompiledPackageInfo(string packageFileName);

        /// <summary>
        /// Installs the package files contained in an umbraco package file (zip)
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="packageFileName"></param>
        /// <param name="userId"></param>
        IEnumerable<string> InstallCompiledPackageFiles(PackageDefinition packageDefinition, string packageFileName, int userId = 0);

        /// <summary>
        /// Installs the data, entities, objects contained in an umbraco package file (zip)
        /// </summary>
        /// <param name="packageDefinition"></param>
        /// <param name="packageFileName"></param>
        /// <param name="userId"></param>
        InstallationSummary InstallCompiledPackageData(PackageDefinition packageDefinition, string packageFileName, int userId = 0);

        #endregion

        #region Installed Packages

        IEnumerable<PackageDefinition> GetAllInstalledPackages();
        PackageDefinition GetInstalledPackageById(int id);
        void DeleteInstalledPackage(int packageId, int userId = 0);

        /// <summary>
        /// Persists a package definition to storage
        /// </summary>
        /// <returns></returns>
        bool SaveInstalledPackage(PackageDefinition definition);

        #endregion

        #region Created Packages

        IEnumerable<PackageDefinition> GetAllCreatedPackages();
        PackageDefinition GetCreatedPackageById(int id);
        void DeleteCreatedPackage(int id, int userId = 0);

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
        Task<string> FetchPackageFileAsync(Guid packageId, Version umbracoVersion, int userId);
    }
}
