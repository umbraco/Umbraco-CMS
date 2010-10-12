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
    public partial class ModuleSelector : System.Web.UI.UserControl
    {
        private cms.businesslogic.packager.repositories.Repository repo;
        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";


        public ModuleSelector()
        {
            this.repo = cms.businesslogic.packager.repositories.Repository.getByGuid(this.repoGuid);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.GetCurrent().GetApplications().Find(t => t.alias.ToLower() == "settings") == null)
            {
                throw new Exception("The current user can't edit skins as the user doesn't have access to the Settings section!");
            }

            LoadModules();
        }

        protected void LoadModules()
        {
            if (this.repo.HasConnection())
            {
                try
                {
                    this.rep_modules.DataSource =  this.repo.Webservice.StarterKitModules();
                    this.rep_modules.DataBind();
                }
                catch (Exception exception)
                {
                    Log.Add(LogTypes.Debug, -1, exception.ToString());
                }
            }
            else
            {
                this.ShowConnectionError();
            }
        }

        private void ShowConnectionError()
        {
            //this.pnl_connectionerror.Visible = true;
        }

        protected void rep_modules_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                cms.businesslogic.packager.repositories.Package p = (cms.businesslogic.packager.repositories.Package)e.Item.DataItem;

                HyperLink link = (HyperLink)e.Item.FindControl("ModuleSelectLink");

                if (cms.businesslogic.skinning.Skinning.IsPackageInstalled(p.RepoGuid) ||
                    cms.businesslogic.skinning.Skinning.IsPackageInstalled(p.Text))
                {                  
                    link.Attributes.Add(
                        "onclick",
                        "umbSelectModule('" + cms.businesslogic.skinning.Skinning.GetModuleAlias(p.Text) + "',this);return false;");

                }
                else
                {
                    link.Attributes.Add(
                        "onclick",
                        "umbInstallModuleAndGetAlias('" + p.RepoGuid +"','"+p.Text+"',this);");
                }


            }
        }

        
    }
}