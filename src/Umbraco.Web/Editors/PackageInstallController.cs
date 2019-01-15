using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Packaging;
using Umbraco.Core.Packaging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using Umbraco.Web.UI.JavaScript;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using File = System.IO.File;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;
using Version = System.Version;

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
            ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache,
            IProfilingLogger logger, IRuntimeState runtimeState)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, applicationCache, logger, runtimeState)
        {
        }

        /// <summary>
        /// This checks if this package & version is alraedy installed
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ValidateInstalled(string name, string version)
        {
            var validate = ValidateInstalledInternal(name, version);
            if (validate == false)
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

                var summary = Services.PackagingService.UninstallPackage(package);

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

        /// <summary>
        /// Returns all installed packages - only shows their latest versions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstalledPackageModel> GetInstalled()
        {
            return Services.PackagingService.GetAllInstalledPackages()
                .GroupBy(
                    //group by name
                    x => x.Name,
                    //select the package with a parsed version
                    pck => Version.TryParse(pck.Version, out var pckVersion)
                        ? new { package = pck, version = pckVersion }
                        : new { package = pck, version = new Version(0, 0, 0) })
                .Select(grouping =>
                {
                    //get the max version for the package
                    var maxVersion = grouping.Max(x => x.version);
                    //only return the first package with this version
                    return grouping.First(x => x.version == maxVersion).package;
                })
                .Select(pack => new InstalledPackageModel
                {
                    Name = pack.Name,
                    Id = pack.Id,
                    Author = pack.Author,
                    Version = pack.Version,
                    Url = pack.Url,
                    License = pack.License,
                    LicenseUrl = pack.LicenseUrl,
                    Files = pack.Files,
                    IconUrl = pack.IconUrl
                })
                .ToList();
        }
        
        private void PopulateFromPackageData(LocalPackageInstallModel model)
        {
            var zipFile = new FileInfo(Path.Combine(IOHelper.MapPath(SystemDirectories.Packages), model.ZipFileName));

            var ins = Services.PackagingService.GetCompiledPackageInfo(zipFile);

            model.Name = ins.Name;
            model.Author = ins.Author;
            model.AuthorUrl = ins.AuthorUrl;
            model.IconUrl = ins.IconUrl;
            model.License = ins.License;
            model.LicenseUrl = ins.LicenseUrl;
            model.Readme = ins.Readme;
            model.ConflictingMacroAliases = ins.Warnings.ConflictingMacros.ToDictionary(x => x.Name, x => x.Alias);
            model.ConflictingStyleSheetNames = ins.Warnings.ConflictingStylesheets.ToDictionary(x => x.Name, x => x.Alias); ;
            model.ConflictingTemplateAliases = ins.Warnings.ConflictingTemplates.ToDictionary(x => x.Name, x => x.Alias); ;
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

        private bool ValidateInstalledInternal(string name, string version)
        {
            var allInstalled = Services.PackagingService.GetAllInstalledPackages();
            var found = allInstalled.FirstOrDefault(x =>
            {
                if (x.Name != name) return false;
                //match the exact version
                if (x.Version == version)
                {
                    return true;
                }
                //now try to compare the versions
                if (Version.TryParse(x.Version, out var installed) && Version.TryParse(version, out var selected))
                {
                    if (installed >= selected) return true;
                }
                return false;
            });
            if (found != null)
            {
                //this package is already installed
                return false;
            }
            return true;
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
                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');
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

                    var validate = ValidateInstalledInternal(model.Name, model.Version);

                    if (validate == false)
                    {
                        //this package is already installed
                        throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                            Services.TextService.Localize("packager/packageAlreadyInstalled")));
                    }

                }
                else
                {
                    model.Notifications.Add(new Notification(
                        Services.TextService.Localize("speechBubbles/operationFailedHeader"),
                        Services.TextService.Localize("media/disallowedFileType"),
                        SpeechBubbleIcon.Warning));
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

            var validate = ValidateInstalledInternal(model.Name, model.Version);

            if (validate == false)
            {
                //this package is already installed
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                    Services.TextService.Localize("packager/packageAlreadyInstalled")));
            }

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
                {
                    throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                        Services.TextService.Localize("packager/targetVersionMismatch", new[] { packageMinVersion.ToString() })));
                }
            }

            var packageDefinition = PackageDefinition.FromCompiledPackage(packageInfo);
            packageDefinition.PackageId = model.PackageGuid; //We must re-map the original package GUID that was generated
            packageDefinition.PackagePath = zipFile.FullName;

            //save to the installedPackages.config
            Services.PackagingService.SaveInstalledPackage(packageDefinition);

            model.Id = packageDefinition.Id;

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
            Current.RestartAppPool(Request.TryGetHttpContext().Result);

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

            var clientDependencyConfig = new ClientDependencyConfiguration(Logger);
            var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                UmbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");

            zipFile.Delete();

            var redirectUrl = "";
            if (packageInfo.Control.IsNullOrWhiteSpace() == false)
            {
                //fixme: this needs to be replaced with an angular view the installer.aspx no longer exists.
                //redirectUrl = string.Format("/developer/framed/{0}",
                //    Uri.EscapeDataString(
                //        string.Format("/umbraco/developer/Packages/installer.aspx?installing=custominstaller&dir={0}&pId={1}&customControl={2}&customUrl={3}", tempDir, model.Id, ins.Control, ins.Url)));
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
