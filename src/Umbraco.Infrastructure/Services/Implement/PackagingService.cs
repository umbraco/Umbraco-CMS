using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;
using File = System.IO.File;

namespace Umbraco.Cms.Core.Services.Implement;

/// <summary>
///     Represents the Packaging Service, which provides import/export functionality for the Core models of the API
///     using xml representation. This is primarily used by the Package functionality.
/// </summary>
public class PackagingService : IPackagingService
{
    private readonly IAuditService _auditService;
    private readonly ICreatedPackagesRepository _createdPackages;
    private readonly IEventAggregator _eventAggregator;
    private readonly IKeyValueService _keyValueService;
    private readonly IManifestParser _manifestParser;
    private readonly IPackageInstallation _packageInstallation;
    private readonly PackageMigrationPlanCollection _packageMigrationPlans;
    private readonly ICoreScopeProvider _coreScopeProvider;

    public PackagingService(
        IAuditService auditService,
        ICreatedPackagesRepository createdPackages,
        IPackageInstallation packageInstallation,
        IEventAggregator eventAggregator,
        IManifestParser manifestParser,
        IKeyValueService keyValueService,
        PackageMigrationPlanCollection packageMigrationPlans,
        ICoreScopeProvider coreScopeProvider)
    {
        _auditService = auditService;
        _createdPackages = createdPackages;
        _packageInstallation = packageInstallation;
        _eventAggregator = eventAggregator;
        _manifestParser = manifestParser;
        _keyValueService = keyValueService;
        _packageMigrationPlans = packageMigrationPlans;
        _coreScopeProvider = coreScopeProvider;
    }

    [Obsolete("Use the ctor which is not obsolete, scheduled for removal in v15")]
    public PackagingService(
        IAuditService auditService,
        ICreatedPackagesRepository createdPackages,
        IPackageInstallation packageInstallation,
        IEventAggregator eventAggregator,
        IManifestParser manifestParser,
        IKeyValueService keyValueService,
        PackageMigrationPlanCollection packageMigrationPlans)
    : this(
        auditService,
        createdPackages,
        packageInstallation,
        eventAggregator,
        manifestParser,
        keyValueService,
        packageMigrationPlans,
        StaticServiceProvider.Instance.GetRequiredService<ICoreScopeProvider>())
    {
    }

    #region Installation

    public CompiledPackage GetCompiledPackageInfo(XDocument? xml) => _packageInstallation.ReadPackage(xml);

    public InstallationSummary InstallCompiledPackageData(
        XDocument? packageXml,
        int userId = Constants.Security.SuperUserId)
    {
        CompiledPackage compiledPackage = GetCompiledPackageInfo(packageXml);

        if (compiledPackage == null)
        {
            throw new InvalidOperationException("Could not read the package file " + packageXml);
        }

        // Trigger the Importing Package Notification and stop execution if event/user is cancelling it
        var importingPackageNotification = new ImportingPackageNotification(compiledPackage.Name);
        if (_eventAggregator.PublishCancelable(importingPackageNotification))
        {
            return new InstallationSummary(compiledPackage.Name);
        }

        InstallationSummary summary = _packageInstallation.InstallPackageData(compiledPackage, userId, out _);

        _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package data installed for package '{compiledPackage.Name}'.");

        // trigger the ImportedPackage event
        _eventAggregator.Publish(new ImportedPackageNotification(summary).WithStateFrom(importingPackageNotification));

        return summary;
    }

    public InstallationSummary InstallCompiledPackageData(
        FileInfo packageXmlFile,
        int userId = Constants.Security.SuperUserId)
    {
        XDocument xml;
        using (StreamReader streamReader = File.OpenText(packageXmlFile.FullName))
        {
            xml = XDocument.Load(streamReader);
        }

        return InstallCompiledPackageData(xml, userId);
    }

    #endregion

    #region Created/Installed Package Repositories

    public void DeleteCreatedPackage(int id, int userId = Constants.Security.SuperUserId)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();
        PackageDefinition? package = GetCreatedPackageById(id);
        if (package == null)
        {
            return;
        }

        _auditService.Add(AuditType.PackagerUninstall, userId, -1, "Package", $"Created package '{package.Name}' deleted. Package id: {package.Id}");
        _createdPackages.Delete(id);

        scope.Complete();
    }

    public IEnumerable<PackageDefinition?> GetAllCreatedPackages()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        return _createdPackages.GetAll();
    }

    public PackageDefinition? GetCreatedPackageById(int id)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        return _createdPackages.GetById(id);
    }

    public bool SaveCreatedPackage(PackageDefinition definition)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope();

        var success = _createdPackages.SavePackage(definition);
        scope.Complete();
        return success;
    }

    public string ExportCreatedPackage(PackageDefinition definition)
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);
        return _createdPackages.ExportPackage(definition);
    }

    public InstalledPackage? GetInstalledPackageByName(string packageName)
        => GetAllInstalledPackages().Where(x => x.PackageName?.InvariantEquals(packageName) ?? false).FirstOrDefault();

    public IEnumerable<InstalledPackage> GetAllInstalledPackages()
    {
        using ICoreScope scope = _coreScopeProvider.CreateCoreScope(autoComplete: true);

        IReadOnlyDictionary<string, string?>? keyValues = _keyValueService.FindByKeyPrefix(Constants.Conventions.Migrations.KeyValuePrefix);

        var installedPackages = new List<InstalledPackage>();

        // Collect the package from the package migration plans
        foreach (PackageMigrationPlan plan in _packageMigrationPlans)
        {
            InstalledPackage installedPackage;
            if (plan.PackageId is not null && installedPackages.FirstOrDefault(x => x.PackageId == plan.PackageId) is InstalledPackage installedPackageById)
            {
                installedPackage = installedPackageById;
            }
            else if (installedPackages.FirstOrDefault(x => x.PackageName == plan.PackageName) is InstalledPackage installedPackageByName)
            {
                installedPackage = installedPackageByName;

                // Ensure package ID is set
                installedPackage.PackageId ??= plan.PackageId;
            }
            else
            {
                installedPackage = new InstalledPackage
                {
                    PackageId = plan.PackageId,
                    PackageName = plan.PackageName,
                };

                installedPackages.Add(installedPackage);
            }

            if (installedPackage.Version is null &&
                plan.GetType().Assembly.TryGetInformationalVersion(out string? version))
            {
                installedPackage.Version = version;
            }

            // Combine all package migration plans for a package
            var currentPlans = installedPackage.PackageMigrationPlans.ToList();
            if (keyValues is null || keyValues.TryGetValue(Constants.Conventions.Migrations.KeyValuePrefix + plan.Name, out var currentState) is false)
            {
                currentState = null;
            }

            currentPlans.Add(new InstalledPackageMigrationPlans
            {
                CurrentMigrationId = currentState,
                FinalMigrationId = plan.FinalState,
            });

            installedPackage.PackageMigrationPlans = currentPlans;
        }

        // Collect and merge the packages from the manifests
        foreach (PackageManifest package in _manifestParser.GetManifests())
        {
            if (package.PackageId is null && package.PackageName is null)
            {
                continue;
            }

            InstalledPackage installedPackage;
            if (package.PackageId is not null && installedPackages.FirstOrDefault(x => x.PackageId == package.PackageId) is InstalledPackage installedPackageById)
            {
                installedPackage = installedPackageById;

                // Always use package name from manifest
                installedPackage.PackageName = package.PackageName;
            }
            else if (installedPackages.FirstOrDefault(x => x.PackageName == package.PackageName) is InstalledPackage installedPackageByName)
            {
                installedPackage = installedPackageByName;

                // Ensure package ID is set
                installedPackage.PackageId ??= package.PackageId;
            }
            else
            {
                installedPackage = new InstalledPackage
                {
                    PackageId = package.PackageId,
                    PackageName = package.PackageName,
                };

                installedPackages.Add(installedPackage);
            }

            // Set additional values
            installedPackage.AllowPackageTelemetry = package.AllowPackageTelemetry;
            installedPackage.PackageView = package.PackageView;

            if (!string.IsNullOrEmpty(package.Version))
            {
                installedPackage.Version = package.Version;
            }
        }

        // Return all packages with an ID or name in the package.manifest or package migrations
        return installedPackages;
    }

    #endregion
}
