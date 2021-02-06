using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.JavaScript;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using File = System.IO.File;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;
using CharArrays = Umbraco.Core.Constants.CharArrays;
namespace Umbraco.Web.Editors
{
    /// <summary>
    /// A controller used for installing packages and managing all of the data in the packages section in the back office
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Packages)]
    public class PackageInstallController : UmbracoAuthorizedJsonController
    {
        public PackageInstallController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext, ServiceContext services, AppCaches appCaches,
            IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        /// <summary>
        /// This checks if this package & version is already installed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ValidateInstalled(string name, string version)
        {
            var installType = Services.PackagingService.GetPackageInstallType(name, SemVersion.Parse(version), out _);

            if (installType == PackageInstallType.AlreadyInstalled)
                return BadRequest();

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Uninstall(int packageId)
        {
            try
            {
                var package = Services.PackagingService.GetInstalledPackageById(packageId);
                if (package == null) return NotFound();

                var summary = Services.PackagingService.UninstallPackage(package.Name, Security.GetUserId().ResultOr(0));

                //now get all other packages by this name since we'll uninstall all versions
                foreach (var installed in Services.PackagingService.GetAllInstalledPackages()
                    .Where(x => x.Name == package.Name && x.Id != package.Id))
                {
                    //remove from the xml
                    Services.PackagingService.DeleteInstalledPackage(installed.Id, Security.GetUserId().ResultOr(0));
                }
            }
            catch (Exception ex)
            {
                Logger.Error<PackageInstallController>(ex, "Failed to uninstall.");
                throw;
            }

            return Ok();
        }



        private void PopulateFromPackageData(LocalPackageInstallModel model)
        {
            var zipFile = new FileInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.Packages), model.ZipFileName));

            var ins = Services.PackagingService.GetCompiledPackageInfo(zipFile);

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
                if (UmbracoVersion.Current < packageMinVersion)
                {
                    model.IsCompatible = false;
                }
            }
        }

        [HttpPost]
        [FileUploadCleanupFilter(false)]
        public async Task<LocalPackageInstallModel> UploadLocalPackage()
        {
            if (Request.Content.IsMimeMultipartContent() == false)
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var root = IOHelper.MapPath(SystemDirectories.TempFileUploads);
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            //must have a file
            if (result.FileData.Count == 0)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var model = new LocalPackageInstallModel
            {
                //Generate a new package Id for this, we'll use this later for tracking, when persisting, saving the file, etc...
                PackageGuid = Guid.NewGuid()
            };

            //get the files
            foreach (var file in result.FileData)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim(CharArrays.DoubleQuote);
                var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();

                if (ext.InvariantEquals("zip") || ext.InvariantEquals("umb"))
                {
                    //we always save package files to /App_Data/packages/package-guid.umb for processing as a standard so lets copy.

                    var packagesFolder = IOHelper.MapPath(SystemDirectories.Packages);
                    Directory.CreateDirectory(packagesFolder);
                    var packageFile = Path.Combine(packagesFolder, model.PackageGuid + ".umb");
                    File.Copy(file.LocalFileName, packageFile);

                    model.ZipFileName = Path.GetFileName(packageFile);

                    //add to the outgoing model so that all temp files are cleaned up
                    model.UploadedFiles.Add(new ContentPropertyFile
                    {
                        TempFilePath = file.LocalFileName
                    });

                    //Populate the model from the metadata in the package file (zip file)
                    PopulateFromPackageData(model);

                    var installType = Services.PackagingService.GetPackageInstallType(model.Name, SemVersion.Parse(model.Version), out var alreadyInstalled);

                    if (installType == PackageInstallType.AlreadyInstalled)
                    {
                        //this package is already installed
                        throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                            Services.TextService.Localize("packager", "packageAlreadyInstalled")));
                    }

                    model.OriginalVersion = installType == PackageInstallType.Upgrade ? alreadyInstalled.Version : null;

                }
                else
                {
                    model.Notifications.Add(new Notification(
                        Services.TextService.Localize("speechBubbles", "operationFailedHeader"),
                        Services.TextService.Localize("media", "disallowedFileType"),
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
        public async Task<LocalPackageInstallModel> Fetch(string packageGuid)
        {
            //Default path
            string fileName = packageGuid + ".umb";
            if (File.Exists(Path.Combine(IOHelper.MapPath(SystemDirectories.Packages), fileName)) == false)
            {
                var packageFile = await Services.PackagingService.FetchPackageFileAsync(
                    Guid.Parse(packageGuid),
                    UmbracoVersion.Current,
                    Security.GetUserId().ResultOr(0));

                fileName = packageFile.Name;
            }

            var model = new LocalPackageInstallModel
            {
                PackageGuid = Guid.Parse(packageGuid),
                ZipFileName = fileName
            };

            //Populate the model from the metadata in the package file (zip file)
            PopulateFromPackageData(model);

            var installType = Services.PackagingService.GetPackageInstallType(model.Name, SemVersion.Parse(model.Version), out var alreadyInstalled);

            if (installType == PackageInstallType.AlreadyInstalled)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                    Services.TextService.Localize("packager", "packageAlreadyInstalled")));
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
        public PackageInstallModel Import(PackageInstallModel model)
        {
            var zipFile = new FileInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.Packages), model.ZipFileName));

            var packageInfo = Services.PackagingService.GetCompiledPackageInfo(zipFile);

            //now we need to check for version comparison
            if (packageInfo.UmbracoVersionRequirementsType == RequirementsType.Strict)
            {
                var packageMinVersion = packageInfo.UmbracoVersion;
                if (UmbracoVersion.Current < packageMinVersion)
                    throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                        Services.TextService.Localize("packager", "targetVersionMismatch", new[] {packageMinVersion.ToString()})));
            }

            var installType = Services.PackagingService.GetPackageInstallType(packageInfo.Name, SemVersion.Parse(packageInfo.Version), out var alreadyInstalled);

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
                    if (!Services.PackagingService.SaveInstalledPackage(packageDefinition))
                        throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Could not save the package"));

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
            var definition = Services.PackagingService.GetInstalledPackageById(model.Id);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.Id);

            var zipFile = new FileInfo(definition.PackagePath);

            var installedFiles = Services.PackagingService.InstallCompiledPackageFiles(definition, zipFile, Security.GetUserId().ResultOr(0));

            //set a restarting marker and reset the app pool
            UmbracoApplication.Restart(Request.TryGetHttpContext().Result);

            model.IsRestarting = true;

            return model;
        }

        [HttpPost]
        public PackageInstallModel CheckRestart(PackageInstallModel model)
        {
            if (model.IsRestarting == false) return model;

            //check for the key, if it's not there we're are restarted
            if (Request.TryGetHttpContext().Result.Application.AllKeys.Contains("AppPoolRestarting") == false)
            {
                //reset it
                model.IsRestarting = false;
            }
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
            var definition = Services.PackagingService.GetInstalledPackageById(model.Id);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.Id);

            var zipFile = new FileInfo(definition.PackagePath);

            var installSummary = Services.PackagingService.InstallCompiledPackageData(definition, zipFile, Security.GetUserId().ResultOr(0));

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
            var definition = Services.PackagingService.GetInstalledPackageById(model.Id);
            if (definition == null) throw new InvalidOperationException("Not package definition found with id " + model.Id);

            var zipFile = new FileInfo(definition.PackagePath);

            var packageInfo = Services.PackagingService.GetCompiledPackageInfo(zipFile);

            zipFile.Delete();

            //bump cdf to be safe
            var clientDependencyConfig = new ClientDependencyConfiguration(Logger);
            var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                UmbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");

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
