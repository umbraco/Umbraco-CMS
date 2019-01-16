using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Semver;
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
        public async Task<FileInfo> FetchPackageFileAsync(Guid packageId, Version umbracoVersion, int userId)
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
                    return new FileInfo(packageFilePath);
                }
            }

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package {packageId} fetched from {Constants.PackageRepository.DefaultRepositoryId}");
            return null;
        }

        #endregion

        #region Installation

        public CompiledPackage GetCompiledPackageInfo(FileInfo packageFile) => _packageInstallation.ReadPackage(packageFile);

        public IEnumerable<string> InstallCompiledPackageFiles(PackageDefinition packageDefinition, FileInfo packageFile, int userId = 0)
        {
            if (packageDefinition == null) throw new ArgumentNullException(nameof(packageDefinition));
            if (packageDefinition.Id == default) throw new ArgumentException("The package definition has not been persisted");
            if (packageDefinition.Name == default) throw new ArgumentException("The package definition has incomplete information");

            var compiledPackage = GetCompiledPackageInfo(packageFile);
            if (compiledPackage == null) throw new InvalidOperationException("Could not read the package file " + packageFile);

            var files = _packageInstallation.InstallPackageFiles(packageDefinition, compiledPackage, userId).ToList();
            
            SaveInstalledPackage(packageDefinition);

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package files installed for package '{compiledPackage.Name}'.");

            return files;
        }

        public InstallationSummary InstallCompiledPackageData(PackageDefinition packageDefinition, FileInfo packageFile, int userId = 0)
        {
            if (packageDefinition == null) throw new ArgumentNullException(nameof(packageDefinition));
            if (packageDefinition.Id == default) throw new ArgumentException("The package definition has not been persisted");
            if (packageDefinition.Name == default) throw new ArgumentException("The package definition has incomplete information");

            var compiledPackage = GetCompiledPackageInfo(packageFile);
            if (compiledPackage == null) throw new InvalidOperationException("Could not read the package file " + packageFile);

            if (ImportingPackage.IsRaisedEventCancelled(new ImportPackageEventArgs<string>(packageFile.Name, compiledPackage), this))
                return new InstallationSummary { MetaData = compiledPackage };

            var summary = _packageInstallation.InstallPackageData(packageDefinition, compiledPackage, userId);

            SaveInstalledPackage(packageDefinition);

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package data installed for package '{compiledPackage.Name}'.");

            ImportedPackage.RaiseEvent(new ImportPackageEventArgs<InstallationSummary>(summary, compiledPackage, false), this);

            return summary;
        }

        public UninstallationSummary UninstallPackage(PackageDefinition package, int userId = 0)
        {
            var summary = _packageInstallation.UninstallPackage(package, userId);
            
            SaveInstalledPackage(package);

            DeleteInstalledPackage(package.Id, userId);

            // trigger the UninstalledPackage event
            UninstalledPackage.RaiseEvent(new UninstallPackageEventArgs<UninstallationSummary>(summary, package, false), this);

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

        public PackageDefinition GetInstalledPackageByName(string name)
        {
            var found = _installedPackages.GetAll().FirstOrDefault(x => x.Name.InvariantEquals(name));
            return found;
        }

        public PackageInstallType GetPackageInstallType(string packageName, SemVersion packageVersion, out PackageDefinition alreadyInstalled)
        {
            if (packageName == null) throw new ArgumentNullException(nameof(packageName));
            if (packageVersion == null) throw new ArgumentNullException(nameof(packageVersion));

            alreadyInstalled = GetInstalledPackageByName(packageName);
            if (alreadyInstalled == null) return PackageInstallType.NewInstall;

            if (!SemVersion.TryParse(alreadyInstalled.Version, out var installedVersion))
                throw new InvalidOperationException("Could not parse the currently installed package version " + alreadyInstalled.Version);

            //compare versions
            if (installedVersion >= packageVersion) return PackageInstallType.AlreadyInstalled;

            //it's an upgrade
            return PackageInstallType.Upgrade;
        }

        public PackageDefinition MergePackageDefinition(PackageDefinition original, PackageDefinition upgrade)
        {
            if (!SemVersion.TryParse(original.Version, out var originalVersion))
                throw new InvalidOperationException("Could not parse the original version");
            if(!SemVersion.TryParse(upgrade.Version, out var upgradeVersion))
                throw new InvalidOperationException("Could not parse the upgrade version");
            if (originalVersion >= upgradeVersion)
                throw new InvalidOperationException("The upgrade version must be higher than the original version");
            if (!original.Name.InvariantEquals(upgrade.Name))
                throw new InvalidOperationException("Cannot merge the package definitions, the package name doesn't match");

            var result = original.Clone();

            result.PackagePath = upgrade.PackagePath;
            result.PackageId = upgrade.PackageId;

            result.UmbracoVersion = upgrade.UmbracoVersion;
            result.Version = upgrade.Version;
            result.Url = upgrade.Url;
            result.Readme = upgrade.Readme;
            result.AuthorUrl = upgrade.AuthorUrl;
            result.Author = upgrade.Author;
            result.LicenseUrl = upgrade.LicenseUrl;
            result.PackageId = upgrade.PackageId;
            result.Control = upgrade.Control;
            result.IconUrl = upgrade.IconUrl;
            result.License = upgrade.License;
            result.ContentNodeId = upgrade.ContentNodeId;
            result.ContentLoadChildNodes = upgrade.ContentLoadChildNodes;

            result.Files = original.Files.Concat(upgrade.Files).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            result.DataTypes = original.DataTypes.Concat(upgrade.DataTypes).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            result.Templates = original.Templates.Concat(upgrade.Templates).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            result.Languages = original.Languages.Concat(upgrade.Languages).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();            
            result.Macros = original.Macros.Concat(upgrade.Macros).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            result.Stylesheets = original.Stylesheets.Concat(upgrade.Stylesheets).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();
            result.DocumentTypes = original.DocumentTypes.Concat(upgrade.DocumentTypes).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList(); 
            result.DictionaryItems = original.DictionaryItems.Concat(upgrade.DictionaryItems).Distinct(StringComparer.InvariantCultureIgnoreCase).ToList();

            var originalActions = CompiledPackageXmlParser.GetPackageActions(XElement.Parse(result.Actions), result.Name);
            var upgradeActions = CompiledPackageXmlParser.GetPackageActions(XElement.Parse(upgrade.Actions), result.Name).ToList();
            var upgradeActionsLookup = upgradeActions.ToLookup(x => x.Alias, x => x.XmlData.ToString());

            foreach (var originalAction in originalActions)
            {
                var upgradeActionsWithAlias = upgradeActionsLookup[originalAction.Alias];

                //check if the original action does not exist already in our upgrade actions
                if(upgradeActionsWithAlias.All(upgradeActionXml => upgradeActionXml != originalAction.XmlData.ToString()))
                    upgradeActions.Add(originalAction);
            }
            result.Actions = new XElement("actions", upgradeActions.Select(x => x.XmlData)).ToString();

            return result;
        }

        public bool SaveInstalledPackage(PackageDefinition definition) => _installedPackages.SavePackage(definition);

        public void DeleteInstalledPackage(int packageId, int userId = 0)
        {
            var package = GetInstalledPackageById(packageId);
            if (package == null) return;

            _auditService.Add(AuditType.PackagerUninstall, userId, -1, "Package", $"Installed package '{package.Name}' deleted. Package id: {package.Id}");
            _installedPackages.Delete(packageId);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs before Importing umbraco package
        /// </summary>
        public static event TypedEventHandler<IPackagingService, ImportPackageEventArgs<string>> ImportingPackage;

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
