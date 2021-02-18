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
using Constants = Umbraco.Cms.Core.Constants;

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
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult ValidateInstalled(string name, string version)
        {
            var installType = _packagingService.GetPackageInstallType(name, SemVersion.Parse(version), out _);

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
            model.Author = ins.Author;
            model.AuthorUrl = ins.AuthorUrl;
            model.Contributors = ins.Contributors;
            model.IconUrl = ins.IconUrl;
            model.License = ins.License;
            model.LicenseUrl = ins.LicenseUrl;
            model.Readme = ins.Readme;
            model.ConflictingMacroAliases = ins.Warnings.ConflictingMacros.ToDictionary(x => x.Name, x => x.Alias);
            model.ConflictingStyleSheetNames = ins.Warnings.ConflictingStylesheets.ToDictionary(x => x.Name, x => x.Alias);
            model.ConflictingTemplateAliases = ins.Warnings.ConflictingTemplates.ToDictionary(x => x.Name, x => x.Alias);
            model.ContainsUnsecureFiles = ins.Warnings.UnsecureFiles.Any();
            model.Url = ins.Url;
            model.Version = ins.Version;

            model.UmbracoVersion = ins.UmbracoVersionRequirementsType == RequirementsType.Strict
                ? ins.UmbracoVersion.ToString(3)
                : string.Empty;

            //now we need to check for version comparison
            model.IsCompatible = true;
            if (ins.UmbracoVersionRequirementsType == RequirementsType.Strict)
            {
                var packageMinVersion = ins.UmbracoVersion;
                if (_umbracoVersion.Current < packageMinVersion)
                {
                    model.IsCompatible = false;
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<LocalPackageInstallModel>> UploadLocalPackage(List<IFormFile> file)
        {
            //must have a file
            if (file.Count == 0)
                return NotFound();

            var model = new LocalPackageInstallModel
            {
                //Generate a new package Id for this, we'll use this later for tracking, when persisting, saving the file, etc...
                PackageGuid = Guid.NewGuid()
            };

            //get the files
            foreach (var formFile in file)
            {
                var fileName = formFile.FileName.Trim('\"');
                var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();

                if (ext.InvariantEquals("zip") || ext.InvariantEquals("umb"))
                {
                    //we always save package files to /App_Data/packages/package-guid.umb for processing as a standard so lets copy.

                    var packagesFolder = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Packages);
                    Directory.CreateDirectory(packagesFolder);
                    var packageFile = Path.Combine(packagesFolder, model.PackageGuid + ".umb");

                    using (var stream = System.IO.File.Create(packageFile))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    model.ZipFileName = Path.GetFileName(packageFile);

                    //Populate the model from the metadata in the package file (zip file)
                    PopulateFromPackageData(model);

                    var installType = _packagingService.GetPackageInstallType(model.Name, SemVersion.Parse(model.Version), out var alreadyInstalled);

                    if (installType == PackageInstallType.AlreadyInstalled)
                    {
                        //this package is already installed
                        return ValidationErrorResult.CreateNotificationValidationErrorResult(
                            _localizedTextService.Localize("packager/packageAlreadyInstalled"));
                    }

                    model.OriginalVersion = installType == PackageInstallType.Upgrade ? alreadyInstalled.Version : null;

                }
                else
                {
                    model.Notifications.Add(new BackOfficeNotification(
                        _localizedTextService.Localize("speechBubbles/operationFailedHeader"),
                        _localizedTextService.Localize("media/disallowedFileType"),
                        NotificationStyle.Warning));
                }

            }

            return model;

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
                    _umbracoVersion.Current,
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

            var installType = _packagingService.GetPackageInstallType(model.Name, SemVersion.Parse(model.Version), out var alreadyInstalled);

            if (installType == PackageInstallType.AlreadyInstalled)
            {
                return ValidationErrorResult.CreateNotificationValidationErrorResult(
                    _localizedTextService.Localize("packager/packageAlreadyInstalled"));
            }

            model.OriginalVersion = installType == PackageInstallType.Upgrade ? alreadyInstalled.Version : null;

            return model;
        }

        /// <summary>
        /// Extracts the package zip and gets the packages information
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<PackageInstallModel> Import(PackageInstallModel model)
        {
            var zipFile = new FileInfo(Path.Combine(_hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Packages), model.ZipFileName));

            var packageInfo = _packagingService.GetCompiledPackageInfo(zipFile);

            //now we need to check for version comparison
            if (packageInfo.UmbracoVersionRequirementsType == RequirementsType.Strict)
            {
                var packageMinVersion = packageInfo.UmbracoVersion;
                if (_umbracoVersion.Current < packageMinVersion)
                    return ValidationErrorResult.CreateNotificationValidationErrorResult(
                        _localizedTextService.Localize("packager/targetVersionMismatch", new[] {packageMinVersion.ToString()}));
            }

            var installType = _packagingService.GetPackageInstallType(packageInfo.Name, SemVersion.Parse(packageInfo.Version), out var alreadyInstalled);

            var packageDefinition = PackageDefinition.FromCompiledPackage(packageInfo);
            packageDefinition.PackagePath = zipFile.FullName;
            packageDefinition.PackageId = model.PackageGuid; //We must re-map the original package GUID that was generated

            switch (installType)
            {
                case PackageInstallType.AlreadyInstalled:
                    throw new InvalidOperationException("The package is already installed");
                case PackageInstallType.NewInstall:
                case PackageInstallType.Upgrade:

                    //save to the installedPackages.config, this will create a new entry with a new Id
                    if (!_packagingService.SaveInstalledPackage(packageDefinition))
                        return ValidationErrorResult.CreateNotificationValidationErrorResult("Could not save the package");

                    model.Id = packageDefinition.Id;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            return model;
        }

        /// <summary>
        /// Installs the package files
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public PackageInstallModel InstallFiles(PackageInstallModel model)
        {
            var definition = _packagingService.GetInstalledPackageById(model.Id);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.Id);

            var zipFile = new FileInfo(definition.PackagePath);
            var installedFiles = _packagingService.InstallCompiledPackageFiles(definition, zipFile, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            //set a restarting marker and reset the app pool
            _umbracoApplicationLifetime.Restart();

            model.IsRestarting = true;

            return model;
        }

        [HttpPost]
        public PackageInstallModel CheckRestart(PackageInstallModel model)
        {
            if (model.IsRestarting == false) return model;

            model.IsRestarting = _umbracoApplicationLifetime.IsRestarting;

            return model;
        }

        /// <summary>
        /// Installs the packages data/business logic
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public PackageInstallModel InstallData(PackageInstallModel model)
        {
            var definition = _packagingService.GetInstalledPackageById(model.Id);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.Id);

            var zipFile = new FileInfo(definition.PackagePath);
            var installSummary = _packagingService.InstallCompiledPackageData(definition, zipFile, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            return model;
        }

        /// <summary>
        /// Cleans up the package installation
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public PackageInstallResult CleanUp(PackageInstallModel model)
        {
            var definition = _packagingService.GetInstalledPackageById(model.Id);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.Id);

            var zipFile = new FileInfo(definition.PackagePath);

            var packageInfo = _packagingService.GetCompiledPackageInfo(zipFile);

            zipFile.Delete();

            //bump cdf to be safe
            _runtimeMinifier.Reset();

            var redirectUrl = "";
            if (!packageInfo.PackageView.IsNullOrWhiteSpace())
            {
                redirectUrl = $"/packages/packages/options/{model.Id}";
            }

            return new PackageInstallResult
            {
                Id = model.Id,
                ZipFileName = model.ZipFileName,
                PackageGuid = model.PackageGuid,
                PostInstallationPath = redirectUrl
            };

        }


    }
}
