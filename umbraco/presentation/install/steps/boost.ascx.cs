using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.install.steps {
    public partial class boost : System.Web.UI.UserControl {
        
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            //if this is an upgrade, skip this step...
            if (!String.IsNullOrEmpty(GlobalSettings.ConfigurationStatus.Trim()))
                Response.Redirect("default.aspx?installStep=theend");
            

            if (cms.businesslogic.packager.InstalledPackage.isPackageInstalled("ae41aad0-1c30-11dd-bd0b-0800200c9a66")) {
                pl_nitros.Visible = true;
                pl_boost.Visible = false;
                
                ph_nitros.Controls.Add(new UserControl().LoadControl(GlobalSettings.Path + "/developer/packages/LoadNitros.ascx"));

                Button btNext = (Button)Page.FindControl("next");
                btNext.Text = "Continue without installing";
                btNext.OnClientClick = "showProgress(this,'loadingBar'); return true;";
                btNext.Click += btNext_Done;
            } else {
                pl_nitros.Visible = false;
                pl_boost.Visible = true;
                Button btNext = (Button)Page.FindControl("next");
                btNext.OnClientClick = "showProgress(this,'loadingBar'); return true;";
                btNext.Click += new EventHandler(btNext_Click);
            }
        }


        protected override bool OnBubbleEvent(object source, EventArgs args) {
            pl_nitros.Controls.Clear();
            
            HtmlGenericControl modulesInstalledHeader = new HtmlGenericControl("h2");
            modulesInstalledHeader.InnerText = "Runway and modules has been installed";

            uicontrols.Feedback fb = new global::umbraco.uicontrols.Feedback();
            fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
            fb.Text = "<strong>Modules installed successfully.</strong> Each module can be uninstalled under the 'installed packages' section in the umbraco backend";

            Button btNext = (Button)Page.FindControl("next");
            btNext.Text = "Next »";

            pl_nitros.Controls.Add(modulesInstalledHeader);
            pl_nitros.Controls.Add(fb);

            return base.OnBubbleEvent(source, args);
        }


        protected void Page_Load(object sender, EventArgs e) {}

        void btNext_Done(object sender, EventArgs e) {
            Response.Redirect("default.aspx?installStep=theend");        
        }


        void btNext_Click(object sender, EventArgs e) {
            if (rb_install.Checked) {
                string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";
                string packageGuid = "ae41aad0-1c30-11dd-bd0b-0800200c9a66";

                cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();
                cms.businesslogic.packager.repositories.Repository repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);

                if (repo.HasConnection()) {
                    string tempFile = p.Import(repo.fetch(packageGuid));
                    p.LoadConfig(tempFile);
                    int pID = p.CreateManifest(tempFile, packageGuid, repoGuid);

                    p.InstallBusinessLogic(pID, tempFile);
                    p.InstallCleanUp(pID, tempFile);

                    //pushing the content to the cache.. 
                    library.RefreshContent();


                    pl_nitros.Visible = true;
                    pl_boost.Visible = false;
                    ph_nitros.Controls.Add(new UserControl().LoadControl(GlobalSettings.Path + "/developer/packages/LoadNitros.ascx"));

                    Button btNext = (Button)Page.FindControl("next");
                    btNext.Text = "Continue without installing";
                } else {
                    uicontrols.Feedback fb = new global::umbraco.uicontrols.Feedback();
                    fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                    fb.Text = "<strong>No connection to repository.</strong> Runway could not be installed as there was no connection to: '" + repo.RepositoryUrl + "'";
                    pl_boost.Controls.Clear();
                    pl_boost.Controls.Add(fb);
                }

            } else {
                Response.Redirect("default.aspx?installStep=theend");
            }
        }
    }
}