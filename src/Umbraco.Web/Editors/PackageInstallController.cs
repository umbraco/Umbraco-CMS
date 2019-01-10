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
using Umbraco.Web._Legacy.Packager;
using Umbraco.Web._Legacy.Packager.PackageInstance;
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
        private readonly PackageActionRunner _packageActionRunner;

        public PackageInstallController(IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext, ServiceContext services, CacheHelper applicationCache,
            IProfilingLogger logger, IRuntimeState runtimeState, PackageActionRunner packageActionRunner)
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
                var pack = InstalledPackage.GetById(packageId);
                if (pack == null) return NotFound();

                PerformUninstall(pack);

                //now get all other packages by this name since we'll uninstall all versions
                foreach (var installed in InstalledPackage.GetAllInstalledPackages()
                    .Where(x => x.Data.Name == pack.Data.Name && x.Data.Id != pack.Data.Id))
                {
                    //remove from teh xml
                    installed.Delete(Security.GetUserId().ResultOr(0));
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
        /// <param name="pack"></param>
        protected void PerformUninstall(InstalledPackage pack)
        {
            if (pack == null) throw new ArgumentNullException("pack");

            var removedTemplates = new List<ITemplate>();
            var removedMacros = new List<IMacro>();
            var removedContentTypes = new List<IContentType>();
            var removedDictionaryItems = new List<IDictionaryItem>();
            var removedDataTypes = new List<IDataType>();
            var removedFiles = new List<string>();

            //Uninstall templates
            foreach (var item in pack.Data.Templates.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var found = Services.FileService.GetTemplate(nId);
                if (found != null)
                {
                    removedTemplates.Add(found);
                    Services.FileService.DeleteTemplate(found.Alias, Security.GetUserId().ResultOr(0));
                }
                pack.Data.Templates.Remove(nId.ToString());
            }

            //Uninstall macros
            foreach (var item in pack.Data.Macros.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var macro = Services.MacroService.GetById(nId);
                if (macro != null)
                {
                    removedMacros.Add(macro);
                    Services.MacroService.Delete(macro);
                }
                pack.Data.Macros.Remove(nId.ToString());
            }

            //Remove Document Types
            var contentTypes = new List<IContentType>();
            var contentTypeService = Services.ContentTypeService;
            foreach (var item in pack.Data.DocumentTypes.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var contentType = contentTypeService.Get(nId);
                if (contentType == null) continue;
                contentTypes.Add(contentType);
                pack.Data.DocumentTypes.Remove(nId.ToString(CultureInfo.InvariantCulture));
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
            foreach (var item in pack.Data.DictionaryItems.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var di = Services.LocalizationService.GetDictionaryItemById(nId);
                if (di != null)
                {
                    removedDictionaryItems.Add(di);
                    Services.LocalizationService.Delete(di);
                }
                pack.Data.DictionaryItems.Remove(nId.ToString());
            }

            //Remove Data types
            foreach (var item in pack.Data.DataTypes.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var dtd = Services.DataTypeService.GetDataType(nId);
                if (dtd != null)
                {
                    removedDataTypes.Add(dtd);
                    Services.DataTypeService.Delete(dtd);
                }
                pack.Data.DataTypes.Remove(nId.ToString());
            }

            pack.Save();

            // uninstall actions
            //TODO: We should probably report errors to the UI!!
            // This never happened before though, but we should do something now
            if (pack.Data.Actions.IsNullOrWhiteSpace() == false)
            {
                try
                {
                    var actionsXml = new XmlDocument();
                    actionsXml.LoadXml("<Actions>" + pack.Data.Actions + "</Actions>");

                    Logger.Debug<PackageInstallController>("Executing undo actions: {UndoActionsXml}", actionsXml.OuterXml);

                    foreach (XmlNode n in actionsXml.DocumentElement.SelectNodes("//Action"))
                    {
                        try
                        {
                            _packageActionRunner.UndoPackageAction(pack.Data.Name, n.Attributes["alias"].Value, n);
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
            foreach (var item in pack.Data.Files.ToArray())
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
                pack.Data.Files.Remove(file);
            }
            pack.Save();
            pack.Delete(Security.GetUserId().ResultOr(0));

            // create a summary of what was actually removed, for PackagingService.UninstalledPackage
            var summary = new UninstallationSummary
            {
                MetaData = pack.GetMetaData(),
                TemplatesUninstalled = removedTemplates,
                MacrosUninstalled = removedMacros,
                ContentTypesUninstalled = removedContentTypes,
                DictionaryItemsUninstalled = removedDictionaryItems,
                DataTypesUninstalled = removedDataTypes,
                FilesUninstalled = removedFiles,
                PackageUninstalled = true
            };

            // trigger the UninstalledPackage event
            PackagingService.OnUninstalledPackage(new UninstallPackageEventArgs<UninstallationSummary>(summary, false));

        }

        /// <summary>
        /// Returns all installed packages - only shows their latest versions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InstalledPackageModel> GetInstalled()
        {
            return InstalledPackage.GetAllInstalledPackages()
                .GroupBy(
                    //group by name
                    x => x.Data.Name,
                    //select the package with a parsed version
                    pck =>
                    {
                        Version pckVersion;
                        return Version.TryParse(pck.Data.Version, out pckVersion)
                            ? new { package = pck, version = pckVersion }
                            : new { package = pck, version = new Version(0, 0, 0) };
                    })
                .Select(grouping =>
                {
                    //get the max version for the package
                    var maxVersion = grouping.Max(x => x.version);
                    //only return the first package with this version
                    return grouping.First(x => x.version == maxVersion).package;
                })
                .Select(pack => new InstalledPackageModel
                {
                    Name = pack.Data.Name,
                    Id = pack.Data.Id,
                    Author = pack.Data.Author,
                    Version = pack.Data.Version,
                    Url = pack.Data.Url,
                    License = pack.Data.License,
                    LicenseUrl = pack.Data.LicenseUrl,
                    Files = pack.Data.Files,
                    IconUrl = pack.Data.IconUrl
                })
                .ToList();
        }
        
        private void PopulateFromPackageData(LocalPackageInstallModel model)
        {
            var ins = new global::Umbraco.Web._Legacy.Packager.Installer(Security.CurrentUser.Id);
            //this will load in all the metadata too
            var tempDir = ins.Import(model.ZipFilePath, false);

            model.TemporaryDirectoryPath = Path.Combine(SystemDirectories.Data, tempDir);
            model.Name = ins.Name;
            model.Author = ins.Author;
            model.AuthorUrl = ins.AuthorUrl;
            model.IconUrl = ins.IconUrl;
            model.License = ins.License;
            model.LicenseUrl = ins.LicenseUrl;
            model.ReadMe = ins.ReadMe;
            model.ConflictingMacroAliases = ins.ConflictingMacroAliases;
            model.ConflictingStyleSheetNames = ins.ConflictingStyleSheetNames;
            model.ConflictingTemplateAliases = ins.ConflictingTemplateAliases;
            model.ContainsBinaryFileErrors = ins.ContainsBinaryFileErrors;
            model.ContainsMacroConflict = ins.ContainsMacroConflict;
            model.ContainsStyleSheetConflicts = ins.ContainsStyleSheeConflicts;
            model.ContainsTemplateConflicts = ins.ContainsTemplateConflicts;
            model.ContainsUnsecureFiles = ins.ContainsUnsecureFiles;
            model.Url = ins.Url;
            model.Version = ins.Version;

            model.UmbracoVersion = ins.RequirementsType == RequirementsType.Strict
                ? string.Format("{0}.{1}.{2}", ins.RequirementsMajor, ins.RequirementsMinor, ins.RequirementsPatch)
                : string.Empty;

            //now we need to check for version comparison
            model.IsCompatible = true;
            if (ins.RequirementsType == RequirementsType.Strict)
            {
                var packageMinVersion = new System.Version(ins.RequirementsMajor, ins.RequirementsMinor, ins.RequirementsPatch);
                if (UmbracoVersion.Current < packageMinVersion)
                {
                    model.IsCompatible = false;
                }
            }
        }

        private bool ValidateInstalledInternal(string name, string version)
        {
            var allInstalled = InstalledPackage.GetAllInstalledPackages();
            var found = allInstalled.FirstOrDefault(x =>
            {
                if (x.Data.Name != name) return false;
                //match the exact version
                if (x.Data.Version == version)
                {
                    return true;
                }
                //now try to compare the versions
                Version installed;
                Version selected;
                if (Version.TryParse(x.Data.Version, out installed) && Version.TryParse(version, out selected))
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
        public LocalPackageInstallModel Fetch(string packageGuid)
        {
            //Default path
            string path = Path.Combine("packages", packageGuid + ".umb");
            if (File.Exists(IOHelper.MapPath(Path.Combine(SystemDirectories.Data, path))) == false)
            {
                path = Services.PackagingService.FetchPackageFile(Guid.Parse(packageGuid), UmbracoVersion.Current, Security.GetUserId().ResultOr(0));
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
            var ins = new global::Umbraco.Web._Legacy.Packager.Installer(Security.CurrentUser.Id);

            var tempPath = ins.Import(model.ZipFilePath);
            //now we need to check for version comparison
            if (ins.RequirementsType == RequirementsType.Strict)
            {
                var packageMinVersion = new System.Version(ins.RequirementsMajor, ins.RequirementsMinor, ins.RequirementsPatch);
                if (UmbracoVersion.Current < packageMinVersion)
                {
                    throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(
                        Services.TextService.Localize("packager/targetVersionMismatch", new[] { packageMinVersion.ToString() })));
                }
            }

            model.TemporaryDirectoryPath = Path.Combine(SystemDirectories.Data, tempPath);
            model.Id = ins.CreateManifest(IOHelper.MapPath(model.TemporaryDirectoryPath), model.PackageGuid, model.RepositoryGuid.ToString());

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
            var ins = new global::Umbraco.Web._Legacy.Packager.Installer(Security.CurrentUser.Id);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallFiles(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));

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
            var ins = new global::Umbraco.Web._Legacy.Packager.Installer(Security.CurrentUser.Id);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallBusinessLogic(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));
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
            var ins = new global::Umbraco.Web._Legacy.Packager.Installer(Security.CurrentUser.Id);
            var tempDir = IOHelper.MapPath(model.TemporaryDirectoryPath);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallCleanUp(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));

            var clientDependencyConfig = new ClientDependencyConfiguration(Logger);
            var clientDependencyUpdated = clientDependencyConfig.UpdateVersionNumber(
                UmbracoVersion.SemanticVersion, DateTime.UtcNow, "yyyyMMdd");


            var redirectUrl = "";
            if (ins.Control.IsNullOrWhiteSpace() == false)
            {
                //fixme: this needs to be replaced with an angular view the installer.aspx no longer exists.
                redirectUrl = string.Format("/developer/framed/{0}",
                    Uri.EscapeDataString(
                        string.Format("/umbraco/developer/Packages/installer.aspx?installing=custominstaller&dir={0}&pId={1}&customControl={2}&customUrl={3}", tempDir, model.Id, ins.Control, ins.Url)));
            }

            return new PackageInstallResult
            {
                Id = model.Id,
                ZipFilePath = model.ZipFilePath,
                PackageGuid = model.PackageGuid,
                RepositoryGuid = model.RepositoryGuid,
                TemporaryDirectoryPath = model.TemporaryDirectoryPath,
                PostInstallationPath = redirectUrl
            };

        }


    }
}
