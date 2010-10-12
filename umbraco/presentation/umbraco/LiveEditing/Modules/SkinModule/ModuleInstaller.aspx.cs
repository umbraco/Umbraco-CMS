using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.packager;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ModuleInstaller : BasePages.UmbracoEnsuredPage
    {
        private cms.businesslogic.packager.repositories.Repository repo;
        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";


        public ModuleInstaller()
        {
            this.repo = cms.businesslogic.packager.repositories.Repository.getByGuid(this.repoGuid);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(Request["guid"])){

                Guid guid = new Guid(Request["guid"]);

                if (this.repo.HasConnection())
                {
                    Installer installer = new Installer();
                    string tempDir = installer.Import(this.repo.fetch(guid.ToString()));
                    installer.LoadConfig(tempDir);
                    int packageId = installer.CreateManifest(tempDir, guid.ToString(), this.repoGuid);
                    installer.InstallFiles(packageId, tempDir);
                    installer.InstallBusinessLogic(packageId, tempDir);
                    installer.InstallCleanUp(packageId, tempDir);
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