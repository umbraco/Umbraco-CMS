using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Extensions;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Constants = Umbraco.Cms.Core.Constants;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// A controller used for managing packages in the back office
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessPackages)]
    public class PackageController : UmbracoAuthorizedJsonController
    {
        private readonly IPackagingService _packagingService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IKeyValueService _keyValueService;
        private readonly PendingPackageMigrations _pendingPackageMigrations;
        private readonly PackageMigrationPlanCollection _packageMigrationPlans;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IScopeProvider _scopeProvider;
        private readonly ILogger<PackageController> _logger;

        public PackageController(
            IPackagingService packagingService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IKeyValueService keyValueService,
            PendingPackageMigrations pendingPackageMigrations,
            PackageMigrationPlanCollection packageMigrationPlans,
            IMigrationPlanExecutor migrationPlanExecutor,
            IScopeProvider scopeProvider,
            ILogger<PackageController> logger)
        {
            _packagingService = packagingService ?? throw new ArgumentNullException(nameof(packagingService));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _keyValueService = keyValueService;
            _pendingPackageMigrations = pendingPackageMigrations;
            _packageMigrationPlans = packageMigrationPlans;
            _migrationPlanExecutor = migrationPlanExecutor;
            _scopeProvider = scopeProvider;
            _logger = logger;
        }

        public IEnumerable<PackageDefinition> GetCreatedPackages()
        {
            return _packagingService.GetAllCreatedPackages();
        }

        public ActionResult<PackageDefinition> GetCreatedPackageById(int id)
        {
            var package = _packagingService.GetCreatedPackageById(id);
            if (package == null)
                return NotFound();

            return package;
        }

        public PackageDefinition GetEmpty()
        {
            return new PackageDefinition();
        }

        /// <summary>
        /// Creates or updates a package
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult<PackageDefinition> PostSavePackage(PackageDefinition model)
        {
            if (ModelState.IsValid == false)
                return new ValidationErrorResult(new SimpleValidationModel(ModelState.ToErrorDictionary()));

            //save it
            if (!_packagingService.SaveCreatedPackage(model))
                return ValidationErrorResult.CreateNotificationValidationErrorResult(
                        model.Id == default
                            ? $"A package with the name {model.Name} already exists"
                            : $"The package with id {model.Id} was not found");

            _packagingService.ExportCreatedPackage(model);

            //the packagePath will be on the model
            return model;
        }

        /// <summary>
        /// Deletes a created package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns></returns>
        [HttpPost]
        [HttpDelete]
        public IActionResult DeleteCreatedPackage(int packageId)
        {
            _packagingService.DeleteCreatedPackage(packageId, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            return Ok();
        }

        [HttpPost]
        public ActionResult<IEnumerable<InstalledPackage>> RunMigrations([FromQuery]string packageName)
        {
            IReadOnlyDictionary<string, string> keyValues = _keyValueService.FindByKeyPrefix(Constants.Conventions.Migrations.KeyValuePrefix);
            IReadOnlyList<string> pendingMigrations = _pendingPackageMigrations.GetPendingPackageMigrations(keyValues);
            foreach(PackageMigrationPlan plan in _packageMigrationPlans.Where(x => x.PackageName.InvariantEquals(packageName)))
            {
                if (pendingMigrations.Contains(plan.Name))
                {
                    var upgrader = new Upgrader(plan);

                    try
                    {
                        upgrader.Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Package migration failed on package {Package} for plan {Plan}", packageName, plan.Name);

                        return ValidationErrorResult.CreateNotificationValidationErrorResult(
                            $"Package migration failed on package {packageName} for plan {plan.Name} with error: {ex.Message}. Check log for full details.");
                    }
                }
            }

            return _packagingService.GetAllInstalledPackages().ToList();
        }

        [HttpGet]
        public IActionResult DownloadCreatedPackage(int id)
        {
            var package = _packagingService.GetCreatedPackageById(id);
            if (package == null)
                return NotFound();

            if (!System.IO.File.Exists(package.PackagePath))
                return ValidationErrorResult.CreateNotificationValidationErrorResult("No file found for path " + package.PackagePath);

            var fileName = Path.GetFileName(package.PackagePath);

            var encoding = Encoding.UTF8;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName =  WebUtility.UrlEncode(fileName),
                Inline = false  // false = prompt the user for downloading;  true = browser to try to show the file inline
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            // Set custom header so umbRequestHelper.downloadFile can save the correct filename
            Response.Headers.Add("x-filename", WebUtility.UrlEncode(fileName));
            return new FileStreamResult(System.IO.File.OpenRead(package.PackagePath), new MediaTypeHeaderValue("application/octet-stream")
            {
                Charset = encoding.WebName,
            });

        }

        public ActionResult<InstalledPackage> GetInstalledPackageByName([FromQuery] string packageName)
        {
            InstalledPackage pack = _packagingService.GetInstalledPackageByName(packageName);
            if (pack == null)
            {
                return NotFound();
            }

            return pack;
        }

        /// <summary>
        /// Returns all installed packages - only shows their latest versions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstalledPackage> GetInstalled()
            => _packagingService.GetAllInstalledPackages().ToList();
    }
}
