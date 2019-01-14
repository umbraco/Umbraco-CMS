using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Umbraco.Core.Collections;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Strings;
using Content = Umbraco.Core.Models.Content;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Represents the Packaging Service, which provides import/export functionality for the Core models of the API
    /// using xml representation. This is primarily used by the Package functionality.
    /// </summary>
    public class PackagingService : IPackagingService
    {

        private readonly IPackageInstallation _packageInstallation;
        private readonly IAuditService _auditService;
        private readonly ICreatedPackagesRepository _createdPackages;
        private readonly IInstalledPackagesRepository _installedPackages;
        private static HttpClient _httpClient;

        public PackagingService(
            IAuditService auditService,
            ICreatedPackagesRepository createdPackages,
            IInstalledPackagesRepository installedPackages,
            IPackageInstallation packageInstallation)
        {   
            _auditService = auditService;
            _createdPackages = createdPackages;
            _installedPackages = installedPackages;
            _packageInstallation = packageInstallation;
        }

        #region Package Files

        /// <inheritdoc />
        public async Task<string> FetchPackageFileAsync(Guid packageId, Version umbracoVersion, int userId)
        {
            //includeHidden = true because we don't care if it's hidden we want to get the file regardless
            var url = $"{Constants.PackageRepository.RestApiBaseUrl}/{packageId}?version={umbracoVersion.ToString(3)}&includeHidden=true&asFile=true";
            byte[] bytes;
            try
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }
                bytes = await _httpClient.GetByteArrayAsync(url);
            }
            catch (HttpRequestException ex)
            {
                throw new ConnectionException("An error occuring downloading the package from " + url, ex);
            }

            //successfull
            if (bytes.Length > 0)
            {
                var packagePath = IOHelper.MapPath(SystemDirectories.Packages);

                // Check for package directory
                if (Directory.Exists(packagePath) == false)
                    Directory.CreateDirectory(packagePath);

                var packageFilePath = Path.Combine(packagePath, packageId + ".umb");

                using (var fs1 = new FileStream(packageFilePath, FileMode.Create))
                {
                    fs1.Write(bytes, 0, bytes.Length);
                    return packageId + ".umb";
                }
            }

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package {packageId} fetched from {Constants.PackageRepository.DefaultRepositoryId}");
            return null;
        }

        #endregion

        #region Installation

        public CompiledPackage GetCompiledPackageInfo(string packageFileName) => _packageInstallation.ReadPackage(packageFileName);

        public IEnumerable<string> InstallCompiledPackageFiles(PackageDefinition packageDefinition, string packageFileName, int userId = 0)
        {
            if (packageDefinition == null) throw new ArgumentNullException(nameof(packageDefinition));
            if (packageDefinition.Id == default) throw new ArgumentException("The package definition has not been persisted");
            if (packageDefinition.Name == default) throw new ArgumentException("The package definition has incomplete information");

            var compiledPackage = GetCompiledPackageInfo(packageFileName);
            if (compiledPackage == null) throw new InvalidOperationException("Could not read the package file " + packageFileName);

            var files = _packageInstallation.InstallPackageFiles(packageDefinition, compiledPackage, userId);

            SaveInstalledPackage(packageDefinition);

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package files installed for package '{compiledPackage.Name}'.");

            return files;
        }

        public InstallationSummary InstallCompiledPackageData(PackageDefinition packageDefinition, string packageFileName, int userId = 0)
        {
            if (packageDefinition == null) throw new ArgumentNullException(nameof(packageDefinition));
            if (packageDefinition.Id == default) throw new ArgumentException("The package definition has not been persisted");
            if (packageDefinition.Name == default) throw new ArgumentException("The package definition has incomplete information");

            var compiledPackage = GetCompiledPackageInfo(packageFileName);
            if (compiledPackage == null) throw new InvalidOperationException("Could not read the package file " + packageFileName);

            if (ImportingPackage.IsRaisedEventCancelled(new ImportPackageEventArgs<string>(packageFileName, compiledPackage), this))
                return new InstallationSummary { MetaData = compiledPackage };

            var summary = _packageInstallation.InstallPackageData(packageDefinition, compiledPackage, userId);

            SaveInstalledPackage(packageDefinition);

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package data installed for package '{compiledPackage.Name}'.");

            ImportedPackage.RaiseEvent(new ImportPackageEventArgs<InstallationSummary>(summary, compiledPackage, false), this);

            return summary;
        }

        #endregion

        #region Created/Installed Package Repositories

        public void DeleteCreatedPackage(int id, int userId = 0)
        {
            var package = GetCreatedPackageById(id);
            if (package == null) return;

            _auditService.Add(AuditType.PackagerUninstall, userId, -1, "Package", $"Created package '{package.Name}' deleted. Package id: {package.Id}");
            _createdPackages.Delete(id);
        }

        public IEnumerable<PackageDefinition> GetAllCreatedPackages() => _createdPackages.GetAll();

        public PackageDefinition GetCreatedPackageById(int id) => _createdPackages.GetById(id);

        public bool SaveCreatedPackage(PackageDefinition definition) => _createdPackages.SavePackage(definition);

        public string ExportCreatedPackage(PackageDefinition definition) => _createdPackages.ExportPackage(definition);


        public IEnumerable<PackageDefinition> GetAllInstalledPackages() => _installedPackages.GetAll();

        public PackageDefinition GetInstalledPackageById(int id) => _installedPackages.GetById(id);

        public bool SaveInstalledPackage(PackageDefinition definition) => _installedPackages.SavePackage(definition);

        public void DeleteInstalledPackage(int packageId, int userId = 0)
        {
            var package = GetInstalledPackageById(packageId);
            if (package == null) return;

            _auditService.Add(AuditType.PackagerUninstall, userId, -1, "Package", $"Installed package '{package.Name}' deleted. Package id: {package.Id}");
            _installedPackages.Delete(packageId);
        }

        #endregion

        /// <summary>
        /// This method can be used to trigger the 'UninstalledPackage' event when a package is uninstalled by something else but this service.
        /// </summary>
        /// <param name="args"></param>
        internal static void OnUninstalledPackage(UninstallPackageEventArgs<UninstallationSummary> args)
        {
            UninstalledPackage.RaiseEvent(args, null);
        }

        #region Event Handlers

        /// <summary>
        /// Occurs before Importing umbraco package
        /// </summary>
        internal static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<string>> ImportingPackage;

        /// <summary>
        /// Occurs after a package is imported
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<InstallationSummary>> ImportedPackage;

        /// <summary>
        /// Occurs after a package is uninstalled
        /// </summary>
        public static event TypedEventHandler<IPackagingService, UninstallPackageEventArgs<UninstallationSummary>> UninstalledPackage;

        #endregion

        
    }
}
