using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using umbraco;
using Umbraco.Core.Auditing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.businesslogic.packager.repositories;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.presentation.developer.packages;
using umbraco.webservices;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using File = System.IO.File;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;
using Settings = umbraco.cms.businesslogic.packager.Settings;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Developer)]
    public class PackageInstallController : UmbracoAuthorizedJsonController
    {
        [HttpPost]
        public IHttpActionResult Uninstall(int packageId)
        {
            var pack = InstalledPackage.GetById(packageId);
            if (pack == null) return NotFound();

            PerformUninstall(pack);

            return Ok();
        }

        /// <summary>
        /// SORRY :( I didn't have time to put this in a service somewhere - the old packager did this all manually too
        /// </summary>
        /// <param name="pack"></param>
        protected void PerformUninstall(InstalledPackage pack)
        {
            if (pack == null) throw new ArgumentNullException("pack");

            var refreshCache = false;
            
            //Uninstall templates
            foreach (var item in pack.Data.Templates.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var found = Services.FileService.GetTemplate(nId);
                if (found != null)
                {
                    ApplicationContext.Services.FileService.DeleteTemplate(found.Alias, Security.GetUserId());
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
                    Services.MacroService.Delete(macro);
                }                    
                pack.Data.Macros.Remove(nId.ToString());
            }

            //Remove Document Types
            var contentTypes = new List<IContentType>();
            var contentTypeService = Services.ContentTypeService;
            foreach (var item in pack.Data.Documenttypes.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var contentType = contentTypeService.GetContentType(nId);
                if (contentType == null) continue;
                contentTypes.Add(contentType);
                pack.Data.Documenttypes.Remove(nId.ToString(CultureInfo.InvariantCulture));
                // refresh content cache when document types are removed
                refreshCache = true;
            }

            //Order the DocumentTypes before removing them
            if (contentTypes.Any())
            {
                var orderedTypes = from contentType in contentTypes
                    orderby contentType.ParentId descending, contentType.Id descending
                    select contentType;
                foreach (var contentType in orderedTypes)
                {
                    contentTypeService.Delete(contentType);
                }
            }

            //Remove Dictionary items
            foreach (var item in pack.Data.DictionaryItems.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var di = Services.LocalizationService.GetDictionaryItemById(nId);
                if (di != null)
                {
                    Services.LocalizationService.Delete(di);
                }                    
                pack.Data.DictionaryItems.Remove(nId.ToString());
            }

            //Remove Data types
            foreach (var item in pack.Data.DataTypes.ToArray())
            {
                int nId;
                if (int.TryParse(item, out nId) == false) continue;
                var dtd = Services.DataTypeService.GetDataTypeDefinitionById(nId);
                if (dtd != null)
                {
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

                    LogHelper.Debug<installedPackage>("executing undo actions: {0}", () => actionsXml.OuterXml);

                    foreach (XmlNode n in actionsXml.DocumentElement.SelectNodes("//Action"))
                    {
                        try
                        {
                            global::umbraco.cms.businesslogic.packager.PackageAction
                                .UndoPackageAction(pack.Data.Name, n.Attributes["alias"].Value, n);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<installedPackage>("An error occurred running undo actions", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<installedPackage>("An error occurred running undo actions", ex);
                }
            }

            //moved remove of files here so custom package actions can still undo
            //Remove files
            foreach (var item in pack.Data.Files.ToArray())
            {
                //here we need to try to find the file in question as most packages does not support the tilde char
                var file = IOHelper.FindFile(item);
                if (file != null)
                {
                    var filePath = IOHelper.MapPath(file);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        
                    }
                }
                pack.Data.Files.Remove(file);
            }
            pack.Save();
            pack.Delete(Security.GetUserId());
            
            //TODO: Legacy - probably not needed
            if (refreshCache)
            {
                library.RefreshContent();
            }            
            TreeDefinitionCollection.Instance.ReRegisterTrees();
            global::umbraco.BusinessLogic.Actions.Action.ReRegisterActionsAndHandlers();
        }


        public IEnumerable<InstalledPackageModel> GetInstalled()
        {
            return data.GetAllPackages(IOHelper.MapPath(Settings.InstalledPackagesSettings))
                .Select(pack => new InstalledPackageModel
                {
                    Name = pack.Name,
                    Id = pack.Id,
                    Author = pack.Author,
                    Version = pack.Version,
                    Url = pack.Url,
                    License = pack.License,
                    LicenseUrl = pack.LicenseUrl,
                    Files = pack.Files
                }).ToList();
        }

        private void PopulateFromPackageData(LocalPackageInstallModel model)
        {
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
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
            model.ContainsLegacyPropertyEditors = ins.ContainsLegacyPropertyEditors;
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

                    //TODO: We need to add the 'strict' requirement to the installer
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
                //our repo guid
                using (var our = Repository.getByGuid("65194810-1f85-11dd-bd0b-0800200c9a66"))
                {
                    path = our.fetch(packageGuid, Security.CurrentUser.Id);    
                }
            }

            var model = new LocalPackageInstallModel
            {
                PackageGuid = Guid.Parse(packageGuid),
                RepositoryGuid = Guid.Parse("65194810-1f85-11dd-bd0b-0800200c9a66"),
                ZipFilePath = path
            };

            //Populate the model from the metadata in the package file (zip file)
            PopulateFromPackageData(model);

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
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);

            var tempPath = ins.Import(model.ZipFilePath);
            //now we need to check for version comparison
            if (ins.RequirementsType == RequirementsType.Strict)
            {
                var packageMinVersion = new System.Version(ins.RequirementsMajor, ins.RequirementsMinor, ins.RequirementsPatch);
                if (UmbracoVersion.Current < packageMinVersion)
                {
                    throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("This package cannot be installed, it requires a minimum Umbraco version of " + packageMinVersion));
                }
            }

            model.TemporaryDirectoryPath = Path.Combine(SystemDirectories.Data, tempPath);
            model.Id = ins.CreateManifest( IOHelper.MapPath(model.TemporaryDirectoryPath), model.PackageGuid.ToString(), model.RepositoryGuid.ToString());

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
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallFiles(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));
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
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
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
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
            var tempDir = IOHelper.MapPath(model.TemporaryDirectoryPath);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallCleanUp(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));

            var clientDependencyConfig = new Umbraco.Core.Configuration.ClientDependencyConfiguration(ApplicationContext.ProfilingLogger.Logger);
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            //clear the tree cache - we'll do this here even though the browser will reload, but just in case it doesn't can't hurt.
            //these bits are super old, but cant find another way to do this currently
            global::umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.ReRegisterTrees();
            global::umbraco.BusinessLogic.Actions.Action.ReRegisterActionsAndHandlers();


            var redirectUrl = "";
            if (ins.Control.IsNullOrWhiteSpace())
            {
                redirectUrl = string.Format("/developer/framed/{0}",
                    Uri.EscapeDataString(
                        string.Format("/umbraco/developer/Packages/installer.aspx?installing=custominstaller&dir={0}&pId={1}&customUrl={2}", tempDir, model.Id, ins.Url)));                
            }
            else
            {
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
