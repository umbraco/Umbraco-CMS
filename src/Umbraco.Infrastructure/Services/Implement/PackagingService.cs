﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services.Implement
{
    /// <summary>
    /// Represents the Packaging Service, which provides import/export functionality for the Core models of the API
    /// using xml representation. This is primarily used by the Package functionality.
    /// </summary>
    public class PackagingService : IPackagingService
    {
        private readonly IPackageInstallation _packageInstallation;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManifestParser _manifestParser;
        private readonly IKeyValueService _keyValueService;
        private readonly PackageMigrationPlanCollection _packageMigrationPlans;
        private readonly IAuditService _auditService;
        private readonly ICreatedPackagesRepository _createdPackages;

        public PackagingService(
            IAuditService auditService,
            ICreatedPackagesRepository createdPackages,
            IPackageInstallation packageInstallation,
            IEventAggregator eventAggregator,
            IManifestParser manifestParser,
            IKeyValueService keyValueService,
            PackageMigrationPlanCollection packageMigrationPlans)
        {
            _auditService = auditService;
            _createdPackages = createdPackages;
            _packageInstallation = packageInstallation;
            _eventAggregator = eventAggregator;
            _manifestParser = manifestParser;
            _keyValueService = keyValueService;
            _packageMigrationPlans = packageMigrationPlans;
        }

        #region Installation

        public CompiledPackage GetCompiledPackageInfo(XDocument xml) => _packageInstallation.ReadPackage(xml);

        public InstallationSummary InstallCompiledPackageData(XDocument packageXml, int userId = Constants.Security.SuperUserId)
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

            var summary = _packageInstallation.InstallPackageData(compiledPackage, userId, out _);

            _auditService.Add(AuditType.PackagerInstall, userId, -1, "Package", $"Package data installed for package '{compiledPackage.Name}'.");

            // trigger the ImportedPackage event
            _eventAggregator.Publish(new ImportedPackageNotification(summary).WithStateFrom(importingPackageNotification));

            return summary;
        }

        public InstallationSummary InstallCompiledPackageData(FileInfo packageXmlFile, int userId = Constants.Security.SuperUserId)
        {
            XDocument xml;
            using (StreamReader streamReader = System.IO.File.OpenText(packageXmlFile.FullName))
            {
                xml = XDocument.Load(streamReader);
            }
            return InstallCompiledPackageData(xml, userId);
        }

        #endregion

        #region Created/Installed Package Repositories

        public void DeleteCreatedPackage(int id, int userId = Cms.Core.Constants.Security.SuperUserId)
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

        public InstalledPackage GetInstalledPackageByName(string packageName)
            => GetAllInstalledPackages().Where(x => x.PackageName.InvariantEquals(packageName)).FirstOrDefault();

        public IEnumerable<InstalledPackage> GetAllInstalledPackages()
        {
            IReadOnlyDictionary<string, string> keyValues = _keyValueService.FindByKeyPrefix(Constants.Conventions.Migrations.KeyValuePrefix);

            var installedPackages = new Dictionary<string, InstalledPackage>();

            // Collect the package from the package migration plans
            foreach(PackageMigrationPlan plan in _packageMigrationPlans)
            {
                if (!installedPackages.TryGetValue(plan.PackageName, out InstalledPackage installedPackage))
                {
                    installedPackage = new InstalledPackage
                    {
                        PackageName = plan.PackageName
                    };
                    installedPackages.Add(plan.PackageName, installedPackage);
                }

                var currentPlans = installedPackage.PackageMigrationPlans.ToList();
                keyValues.TryGetValue(Constants.Conventions.Migrations.KeyValuePrefix + plan.Name, out var currentState);
                currentPlans.Add(new InstalledPackageMigrationPlans
                {
                    CurrentMigrationId = currentState,
                    FinalMigrationId = plan.FinalState
                });

                installedPackage.PackageMigrationPlans = currentPlans;
            }

            // Collect and merge the packages from the manifests
            foreach(PackageManifest package in _manifestParser.GetManifests())
            {
                if (!installedPackages.TryGetValue(package.PackageName, out InstalledPackage installedPackage))
                {
                    installedPackage = new InstalledPackage
                    {
                        PackageName = package.PackageName
                    };
                    installedPackages.Add(package.PackageName, installedPackage);
                }

                installedPackage.PackageView = package.PackageView;
            }

            // Filter the packages listed here. i.e. only packages that have migrations or views.
            // Else whats the point in showing them?
            return installedPackages
                .Values
                .Where(x => !x.PackageView.IsNullOrWhiteSpace() || x.PackageMigrationPlans.Any());
        }

        #endregion
    }
}
