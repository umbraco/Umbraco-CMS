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
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Auditing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager.repositories;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.UI;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Developer)]
    public class PackageInstallController : UmbracoAuthorizedJsonController
    {
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
                    var packageTempFileName = model.PackageGuid + ".umb";
                    var packageTempFileLocation = Path.Combine(packageTempDir, packageTempFileName);
                    File.Copy(file.LocalFileName, packageTempFileLocation, true);

                    try
                    {
                        var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
                        //this will load in all the metadata too
                        var tempDir = ins.Import(packageTempFileName);
                        model.TemporaryDirectoryPath = Path.Combine(SystemDirectories.Data, tempDir);
                        model.Id = ins.CreateManifest(
                            IOHelper.MapPath(model.TemporaryDirectoryPath),
                            model.PackageGuid.ToString(),
                            //TODO: Does this matter? we're installing a local package
                            string.Empty);

                        model.Name = ins.Name;
                        model.Author = ins.Author;
                        model.AuthorUrl = ins.AuthorUrl;
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
                        //TODO: We need to add the 'strict' requirement to the installer
                    }
                    finally
                    {
                        //Cleanup file
                        if (File.Exists(packageTempFileLocation))
                        {
                            File.Delete(packageTempFileLocation);
                        }
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
        public PackageInstallModel Fetch(string packageGuid)
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
            
            var p = new PackageInstallModel();
            p.PackageGuid = Guid.Parse(packageGuid);
            p.RepositoryGuid = Guid.Parse("65194810-1f85-11dd-bd0b-0800200c9a66");
            p.ZipFilePath = path;
            //p.ZipFilePath = Path.Combine("temp", "package.umb");

            return p;
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
            model.TemporaryDirectoryPath = Path.Combine(SystemDirectories.Data, ins.Import(model.ZipFilePath));
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
        public PackageInstallModel CleanUp(PackageInstallModel model)
        {
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallCleanUp(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));

            var clientDependencyConfig = new Umbraco.Core.Configuration.ClientDependencyConfiguration(ApplicationContext.ProfilingLogger.Logger);
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            //clear the tree cache - we'll do this here even though the browser will reload, but just in case it doesn't can't hurt.
            //these bits are super old, but cant find another way to do this currently
            global::umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.ReRegisterTrees();
            global::umbraco.BusinessLogic.Actions.Action.ReRegisterActionsAndHandlers();


            return model;
        }


    }
}
