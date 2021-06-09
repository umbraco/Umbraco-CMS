using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Packaging;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;
using Umbraco.Cms.Core;
namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// A controller used for installing packages and managing all of the data in the packages section in the back office
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessPackages)]
    public class PackageInstallController : UmbracoAuthorizedJsonController
    {

        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoApplicationLifetime _umbracoApplicationLifetime;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly IPackagingService _packagingService;
        private readonly ILogger<PackageInstallController> _logger;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILocalizedTextService _localizedTextService;

        public PackageInstallController(
            IUmbracoVersion umbracoVersion,
            IHostingEnvironment hostingEnvironment,
            IUmbracoApplicationLifetime umbracoApplicationLifetime,
            IRuntimeMinifier runtimeMinifier,
            IPackagingService packagingService,
            ILogger<PackageInstallController> logger,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            ILocalizedTextService localizedTextService)
        {
            _umbracoVersion = umbracoVersion ?? throw new ArgumentNullException(nameof(umbracoVersion));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _umbracoApplicationLifetime = umbracoApplicationLifetime ?? throw new ArgumentNullException(nameof(umbracoApplicationLifetime));
            _runtimeMinifier = runtimeMinifier ?? throw new ArgumentNullException(nameof(runtimeMinifier));
            _packagingService = packagingService ?? throw new ArgumentNullException(nameof(packagingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        }

        /// <summary>
        /// This checks if this package & version is already installed
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ValidateInstalled(string name)
        {
            var installType = _packagingService.GetPackageInstallType(name, out _);

            if (installType == PackageInstallType.AlreadyInstalled)
                return BadRequest();

            return Ok();
        }

        [HttpPost]
        public IActionResult Uninstall(int packageId)
        {
            try
            {

                var package = _packagingService.GetInstalledPackageById(packageId);
                if (package == null) return NotFound();

                var summary = _packagingService.UninstallPackage(package.Name, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

                //now get all other packages by this name since we'll uninstall all versions
                foreach (var installed in _packagingService.GetAllInstalledPackages()
                    .Where(x => x.Name == package.Name && x.Id != package.Id))
                {
                    //remove from the xml
                    _packagingService.DeleteInstalledPackage(installed.Id, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to uninstall.");
                throw;
            }

            return Ok();
        }



        private void PopulateFromPackageData(LocalPackageInstallModel model)
        {
            var zipFile = new FileInfo(Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Packages), model.ZipFileName));

            var ins = _packagingService.GetCompiledPackageInfo(zipFile);

            model.Name = ins.Name;
            model.ConflictingMacroAliases = ins.Warnings.ConflictingMacros.ToDictionary(x => x.Name, x => x.Alias);
            model.ConflictingStyleSheetNames = ins.Warnings.ConflictingStylesheets.ToDictionary(x => x.Name, x => x.Alias);
            model.ConflictingTemplateAliases = ins.Warnings.ConflictingTemplates.ToDictionary(x => x.Name, x => x.Alias);
            model.ContainsUnsecureFiles = ins.Warnings.UnsecureFiles.Any();


            //now we need to check for version comparison
            model.IsCompatible = true;
            if (ins.UmbracoVersionRequirementsType == RequirementsType.Strict)
            {
                //TODO Get this info from Nuget?
                // var packageMinVersion = ins.UmbracoVersion;
                // if (_umbracoVersion.Version < packageMinVersion)
                // {
                //     model.IsCompatible = false;
                // }
            }
        }

        /// <summary>
        /// Gets the package from Our to install
        /// </summary>
        /// <param name="packageGuid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<LocalPackageInstallModel>> Fetch(string packageGuid)
        {
            //Default path
            string fileName = packageGuid + ".umb";
            if (System.IO.File.Exists(Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Packages), fileName)) == false)
            {
                var packageFile = await _packagingService.FetchPackageFileAsync(
                    Guid.Parse(packageGuid),
                    _umbracoVersion.Version,
                    _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

                fileName = packageFile.Name;
            }

            var model = new LocalPackageInstallModel
            {
                PackageGuid = Guid.Parse(packageGuid),
                ZipFileName = fileName
            };

            //Populate the model from the metadata in the package file (zip file)
            PopulateFromPackageData(model);

            var installType = _packagingService.GetPackageInstallType(model.Name, out var alreadyInstalled);

            if (installType == PackageInstallType.AlreadyInstalled)
            {
                return ValidationErrorResult.CreateNotificationValidationErrorResult(
                    _localizedTextService.Localize("packager/packageAlreadyInstalled"));
            }

            return model;
        }
    }
}
