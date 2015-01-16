using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Auditing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager.repositories;
using Umbraco.Core.IO;
using Umbraco.Core.Packaging.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Core.Constants.Applications.Developer)]
    public class PackageInstallController : UmbracoAuthorizedJsonController
    {

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

        [HttpPost]
        public PackageInstallModel Import(PackageInstallModel model)
        {
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
            model.TemporaryDirectoryPath = Path.Combine(SystemDirectories.Data, ins.Import(model.ZipFilePath));
            model.Id = ins.CreateManifest( IOHelper.MapPath(model.TemporaryDirectoryPath), model.PackageGuid.ToString(), model.RepositoryGuid.ToString());

            return model;
        }

        [HttpPost]
        public PackageInstallModel InstallFiles(PackageInstallModel model)
        {
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallFiles(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));
            return model;
        }


        [HttpPost]
        public PackageInstallModel InstallData(PackageInstallModel model)
        {
            var ins = new global::umbraco.cms.businesslogic.packager.Installer(Security.CurrentUser.Id);
            ins.LoadConfig(IOHelper.MapPath(model.TemporaryDirectoryPath));
            ins.InstallBusinessLogic(model.Id, IOHelper.MapPath(model.TemporaryDirectoryPath));
            return model;
        }


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
