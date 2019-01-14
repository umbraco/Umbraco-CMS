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
        private readonly IPackageActionRunner _packageActionRunner;

        public PackageInstallController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache,
            IProfilingLogger logger, IRuntimeState runtimeState, IPackageActionRunner packageActionRunner)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, applicationCache, logger, runtimeState)
        {
            _packageActionRunner = packageActionRunner;
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

                PerformUninstall(package);

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
        /// SORRY :( I didn't have time to put this in a service somewhere - the old packager did this all manually too
        /// </summary>
        /// <param name="package"></param>
        protected void PerformUninstall(PackageDefinition package)
        {
            if (package == null) throw new ArgumentNullException("package");

            var removedTemplates = new List<ITemplate>();
            var removedMacros = new List<IMacro>();
            var removedContentTypes = new List<IContentType>();
            var removedDictionaryItems = new List<IDictionaryItem>();
            var removedDataTypes = new List<IDataType>();
            var removedFiles = new List<string>();

            //Uninstall templates
            foreach (var item in package.Templates.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var found = Services.FileService.GetTemplate(nId);
                if (found != null)
                {
                    removedTemplates.Add(found);
                    Services.FileService.DeleteTemplate(found.Alias, Security.GetUserId().ResultOr(0));
                }
                package.Templates.Remove(nId.ToString());
            }

            //Uninstall macros
            foreach (var item in package.Macros.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var macro = Services.MacroService.GetById(nId);
                if (macro != null)
                {
                    removedMacros.Add(macro);
                    Services.MacroService.Delete(macro);
                }
                package.Macros.Remove(nId.ToString());
            }

            //Remove Document Types
            var contentTypes = new List<IContentType>();
            var contentTypeService = Services.ContentTypeService;
            foreach (var item in package.DocumentTypes.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var contentType = contentTypeService.Get(nId);
                if (contentType == null) continue;
                contentTypes.Add(contentType);
                package.DocumentTypes.Remove(nId.ToString(CultureInfo.InvariantCulture));
            }

            //Order the DocumentTypes before removing them
            if (contentTypes.Any())
            {
                //TODO: I don't think this ordering is necessary
                var orderedTypes = from contentType in contentTypes
                                   orderby contentType.ParentId descending, contentType.Id descending
                                   select contentType;
                removedContentTypes.AddRange(orderedTypes);
                contentTypeService.Delete(orderedTypes);
            }

            //Remove Dictionary items
            foreach (var item in package.DictionaryItems.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var di = Services.LocalizationService.GetDictionaryItemById(nId);
                if (di != null)
                {
                    removedDictionaryItems.Add(di);
                    Services.LocalizationService.Delete(di);
                }
                package.DictionaryItems.Remove(nId.ToString());
            }

            //Remove Data types
            foreach (var item in package.DataTypes.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var dtd = Services.DataTypeService.GetDataType(nId);
                if (dtd != null)
                {
                    removedDataTypes.Add(dtd);
                    Services.DataTypeService.Delete(dtd);
                }
                package.DataTypes.Remove(nId.ToString());
            }

            Services.PackagingService.SaveInstalledPackage(package);

            // uninstall actions
            //TODO: We should probably report errors to the UI!!
            // This never happened before though, but we should do something now
            if (package.Actions.IsNullOrWhiteSpace() == false)
            {
                try
                {
                    var actionsXml = XDocument.Parse("<Actions>" + package.Actions + "</Actions>");

                    Logger.Debug<PackageInstallController>("Executing undo actions: {UndoActionsXml}", actionsXml.ToString(SaveOptions.DisableFormatting));

                    foreach (var n in actionsXml.Root.Elements("Action"))
                    {
                        try
                        {
                            _packageActionRunner.UndoPackageAction(package.Name, n.AttributeValue<string>("alias"), n);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error<PackageInstallController>(ex, "An error occurred running undo actions");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error<PackageInstallController>(ex, "An error occurred running undo actions");
                }
            }

            //moved remove of files here so custom package actions can still undo
            //Remove files
            foreach (var item in package.Files.ToArray())
            {
                removedFiles.Add(item.GetRelativePath());

                //here we need to try to find the file in question as most packages does not support the tilde char
                var file = IOHelper.FindFile(item);
                if (file != null)
                {
                    if (file.StartsWith("/") == false)
                        file = string.Format("/{0}", file);
                    var filePath = IOHelper.MapPath(file);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                }
                package.Files.Remove(file);
            }

            Services.PackagingService.SaveInstalledPackage(package);

            Services.PackagingService.DeleteInstalledPackage(package.Id, Security.GetUserId().ResultOr(0));

            // create a summary of what was actually removed, for PackagingService.UninstalledPackage
            var summary = new UninstallationSummary
            {
                MetaData = package,
                TemplatesUninstalled = removedTemplates,
                MacrosUninstalled = removedMacros,
                ContentTypesUninstalled = removedContentTypes,
                DictionaryItemsUninstalled = removedDictionaryItems,
                DataTypesUninstalled = removedDataTypes,
                FilesUninstalled = removedFiles,
                PackageUninstalled = true
            };

            // trigger the UninstalledPackage event
            // fixme: This all needs to be part of the service!
            PackagingService.OnUninstalledPackage(new UninstallPackageEventArgs<UninstallationSummary>(summary, package, false));

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
            var ins = Services.PackagingService.GetCompiledPackageInfo(model.ZipFilePath);

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
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await Request.Content.ReadAsMultipartAsync(provider);

            //must have a file
            if (result.FileData.Count == 0)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            //TODO: App/Tree Permissions?
            var model = new LocalPackageInstallModel
            {
                PackageGuid = Guid.NewGuid()
            };

            //get the files
            foreach (var file in result.FileData)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' });
                var ext = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();

                //TODO: Only allow .zip
                if (ext.InvariantEquals("zip") || ext.InvariantEquals("umb"))
                {
                    //TODO: Currently it has to be here, it's not ideal but that's the way it is right now
                    var packageTempDir = IOHelper.MapPath(SystemDirectories.Data);

                    //ensure it's there
                    Directory.CreateDirectory(packageTempDir);

                    //copy it - must always be '.umb' for the installer thing to work
                    //the file name must be a GUID - this is what the packager expects (strange yes)
                    //because essentially this is creating a temporary package Id that will be used
                    //for unpacking/installing/etc...
                    model.ZipFilePath = model.PackageGuid + ".umb";
                    var packageTempFileLocation = Path.Combine(packageTempDir, model.ZipFilePath);
                    File.Copy(file.LocalFileName, packageTempFileLocation, true);

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
            string path = Path.Combine("packages", packageGuid + ".umb");
            if (File.Exists(IOHelper.MapPath(Path.Combine(SystemDirectories.Data, path))) == false)
            {
                path = await Services.PackagingService.FetchPackageFileAsync(Guid.Parse(packageGuid), UmbracoVersion.Current, Security.GetUserId().ResultOr(0));
            }

            var model = new LocalPackageInstallModel
            {
                PackageGuid = Guid.Parse(packageGuid),
                RepositoryGuid = Guid.Parse("65194810-1f85-11dd-bd0b-0800200c9a66"),
                ZipFilePath = path
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
            var packageInfo = Services.PackagingService.GetCompiledPackageInfo(model.ZipFilePath);

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

            //save to the installedPackages.config
            packageDefinition.PackageId = model.PackageGuid; //fixme: why are we doing this?
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

            Services.PackagingService.InstallCompiledPackageFiles(definition, model.ZipFilePath, Security.GetUserId().ResultOr(0));

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

            Services.PackagingService.InstallCompiledPackageData(definition, model.ZipFilePath, Security.GetUserId().ResultOr(0));
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
            var packageInfo = Services.PackagingService.GetCompiledPackageInfo(model.ZipFilePath);

            var clientDependencyConfig = new ClientDependencyConfiguration(Logger);
            var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                UmbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");

            //fixme: when do we delete the zip file?

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
                ZipFilePath = model.ZipFilePath,
                PackageGuid = model.PackageGuid,
                RepositoryGuid = model.RepositoryGuid,
                PostInstallationPath = redirectUrl
            };

        }


    }
}
