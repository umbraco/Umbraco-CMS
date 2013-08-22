using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.packager;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ModuleInstaller : BasePages.UmbracoEnsuredPage
    {
        private readonly cms.businesslogic.packager.repositories.Repository _repo;
        private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public ModuleInstaller()
        {
            _repo = cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
            //for skinning, you need to be a developer
            CurrentApp = DefaultApps.developer.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request["guid"]))
            {

                var guid = new Guid(Request["guid"]);

                if (_repo.HasConnection())
                {
                    var installer = new Installer();
                    var tempDir = installer.Import(_repo.fetch(guid.ToString()));
                    installer.LoadConfig(tempDir);
                    var packageId = installer.CreateManifest(tempDir, guid.ToString(), RepoGuid);
                    installer.InstallFiles(packageId, tempDir);
                    installer.InstallBusinessLogic(packageId, tempDir);
                    installer.InstallCleanUp(packageId, tempDir);

                    //NOTE: This seems excessive to have to re-load all content from the database here!?
                    library.RefreshContent();

                    if (cms.businesslogic.skinning.Skinning.IsPackageInstalled(new Guid(Request["guid"])) ||
                        cms.businesslogic.skinning.Skinning.IsPackageInstalled(Request["name"]))
                    {
                        Response.Write(cms.businesslogic.skinning.Skinning.GetModuleAlias(Request["name"]));
                    }
                    else
                    {
                        Response.Write("error");
                    }

                }
                else
                {
                    Response.Write("error");
                }
            }
        }
    }
}