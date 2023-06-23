using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Services.OperationStatus;
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
    private readonly ILegacyManifestParser _legacyManifestParser;
    private readonly IPackageInstallation _packageInstallation;
    private readonly PackageMigrationPlanCollection _packageMigrationPlans;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IUserService _userService;

    public PackagingService(
        IAuditService auditService,
        ICreatedPackagesRepository createdPackages,
        IPackageInstallation packageInstallation,
        IEventAggregator eventAggregator,
        ILegacyManifestParser legacyManifestParser,
        IKeyValueService keyValueService,
        PackageMigrationPlanCollection packageMigrationPlans,
        IHostEnvironment hostEnvironment,
        IUserService userService)
    {
        _auditService = auditService;
        _createdPackages = createdPackages;
        _packageInstallation = packageInstallation;
        _eventAggregator = eventAggregator;
        _legacyManifestParser = legacyManifestParser;
        _keyValueService = keyValueService;
        _packageMigrationPlans = packageMigrationPlans;
        _hostEnvironment = hostEnvironment;
        _userService = userService;
    }

    [Obsolete("Use constructor that also takes an IHostEnvironment and IUserService instead. Scheduled for removal in V15")]
    public PackagingService(
        IAuditService auditService,
        ICreatedPackagesRepository createdPackages,
        IPackageInstallation packageInstallation,
        IEventAggregator eventAggregator,
        ILegacyManifestParser manifestParser,
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
              StaticServiceProvider.Instance.GetRequiredService<IHostEnvironment>(),
              StaticServiceProvider.Instance.GetRequiredService<IUserService>())
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

    [Obsolete("Use DeleteCreatedPackageAsync instead. Scheduled for removal in Umbraco 15.")]
    public void DeleteCreatedPackage(int id, int userId = Constants.Security.SuperUserId)
    {
        PackageDefinition? package = GetCreatedPackageById(id);
        Guid key = package?.PackageId ?? Guid.Empty;
        Guid currentUserKey = _userService.GetUserById(id)?.Key ?? Constants.Security.SuperUserKey;

        DeleteCreatedPackageAsync(key, currentUserKey).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public async Task<Attempt<PackageDefinition?, PackageOperationStatus>> DeleteCreatedPackageAsync(Guid key, Guid userKey)
    {
        PackageDefinition? package = await GetCreatedPackageByKeyAsync(key);
        if (package == null)
        {
            return Attempt.FailWithStatus<PackageDefinition?, PackageOperationStatus>(PackageOperationStatus.NotFound, null);
        }

        int currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
        _auditService.Add(AuditType.Delete, currentUserId, -1, "Package", $"Created package '{package.Name}' deleted. Package key: {key}");
        _createdPackages.Delete(package.Id);

        return Attempt.SucceedWithStatus<PackageDefinition?, PackageOperationStatus>(PackageOperationStatus.Success, package);
    }

    [Obsolete("Use GetCreatedPackagesAsync instead. Scheduled for removal in Umbraco 15.")]
    public IEnumerable<PackageDefinition?> GetAllCreatedPackages() => _createdPackages.GetAll();

    /// <inheritdoc/>
    public async Task<PagedModel<PackageDefinition>> GetCreatedPackagesAsync(int skip, int take)
    {
        PackageDefinition[] packages = _createdPackages.GetAll().WhereNotNull().ToArray();
        var pagedModel = new PagedModel<PackageDefinition>(packages.Length, packages.Skip(skip).Take(take));
        return await Task.FromResult(pagedModel);
    }

    public PackageDefinition? GetCreatedPackageById(int id) => _createdPackages.GetById(id);

    /// <inheritdoc/>
    public Task<PackageDefinition?> GetCreatedPackageByKeyAsync(Guid key) => Task.FromResult(_createdPackages.GetByKey(key));

    [Obsolete("Use CreateCreatedPackageAsync or UpdateCreatedPackageAsync instead. Scheduled for removal in Umbraco 15.")]
    public bool SaveCreatedPackage(PackageDefinition definition) => _createdPackages.SavePackage(definition);

    /// <inheritdoc/>
    public async Task<Attempt<PackageDefinition, PackageOperationStatus>> CreateCreatedPackageAsync(PackageDefinition package, Guid userKey)
    {
        if (_createdPackages.SavePackage(package) == false)
        {
            if (string.IsNullOrEmpty(package.Name))
            {
                return Attempt.FailWithStatus(PackageOperationStatus.InvalidName, package);
            }

            return Attempt.FailWithStatus(PackageOperationStatus.DuplicateItemName, package);
        }

        int currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
        _auditService.Add(AuditType.New, currentUserId, -1, "Package", $"Created package '{package.Name}' created. Package key: {package.PackageId}");
        return await Task.FromResult(Attempt.SucceedWithStatus(PackageOperationStatus.Success, package));

    }

    /// <inheritdoc/>
    public async Task<Attempt<PackageDefinition, PackageOperationStatus>> UpdateCreatedPackageAsync(PackageDefinition package, Guid userKey)
    {
        if (_createdPackages.SavePackage(package) == false)
        {
            return Attempt.FailWithStatus(PackageOperationStatus.NotFound, package);
        }

        int currentUserId = _userService.GetAsync(userKey).Result?.Id ?? Constants.Security.SuperUserId;
        _auditService.Add(AuditType.New, currentUserId, -1, "Package", $"Created package '{package.Name}' updated. Package key: {package.PackageId}");
        return await Task.FromResult(Attempt.SucceedWithStatus(PackageOperationStatus.Success, package));
    }

    public string ExportCreatedPackage(PackageDefinition definition) => _createdPackages.ExportPackage(definition);

    public InstalledPackage? GetInstalledPackageByName(string packageName)
        => GetAllInstalledPackages().Where(x => x.PackageName?.InvariantEquals(packageName) ?? false).FirstOrDefault();

    public IEnumerable<InstalledPackage> GetAllInstalledPackages()
    {
        // Collect the packages from the package migration plans
        var installedPackages = GetInstalledPackagesFromMigrationPlansAsync(0, int.MaxValue)
            .GetAwaiter()
            .GetResult()
            .Items.ToDictionary(package => package.PackageName!, package => package); // PackageName cannot be null here

        // Collect and merge the packages from the manifests
        foreach (LegacyPackageManifest package in _legacyManifestParser.GetManifests())
        {
            if (package.PackageName is null)
            {
                continue;
            }

            if (!installedPackages.TryGetValue(package.PackageName, out InstalledPackage? installedPackage))
            {
                installedPackage = new InstalledPackage
                {
                    PackageName = package.PackageName,
                    Version = string.IsNullOrEmpty(package.Version) ? "Unknown" : package.Version,
                };

                installedPackages.Add(package.PackageName, installedPackage);
            }

            installedPackage.PackageView = package.PackageView;
        }

        // Return all packages with a name in the package.manifest or package migrations
        return installedPackages.Values;
    }

    #endregion

    /// <inheritdoc/>
    public async Task<PagedModel<InstalledPackage>> GetInstalledPackagesFromMigrationPlansAsync(int skip, int take)
    {
        IReadOnlyDictionary<string, string?>? keyValues =
            _keyValueService.FindByKeyPrefix(Constants.Conventions.Migrations.KeyValuePrefix);

        InstalledPackage[] installedPackages = _packageMigrationPlans
            .GroupBy(plan => plan.PackageName)
            .Select(group =>
            {
                var package = new InstalledPackage
                {
                    PackageName = group.Key,
                };

                var currentState = keyValues?
                    .GetValueOrDefault(Constants.Conventions.Migrations.KeyValuePrefix + package.PackageName);

                package.PackageMigrationPlans = group
                    .Select(plan => new InstalledPackageMigrationPlans
                    {
                        CurrentMigrationId = currentState,
                        FinalMigrationId = plan.FinalState,
                    });

                return package;
            }).ToArray();

        return await Task.FromResult(new PagedModel<InstalledPackage>
        {
            Total = installedPackages.Count(),
            Items = installedPackages.Skip(skip).Take(take),
        });
    }

    /// <inheritdoc/>
    public Stream? GetPackageFileStream(PackageDefinition definition)
    {
        // Removing the ContentRootPath from the package path as a sub path is required for GetFileInfo()
        var subPath = definition.PackagePath.Replace(_hostEnvironment.ContentRootPath, string.Empty);

        IFileInfo packageFile = _hostEnvironment.ContentRootFileProvider.GetFileInfo(subPath);

        if (packageFile.Exists == false)
        {
            return null;
        }

        return packageFile.CreateReadStream();
    }
}
