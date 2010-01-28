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
using umbraco.cms.helpers;
using umbraco.IO;

namespace umbraco.presentation.developer.packages {
    public partial class Boost : BasePages.UmbracoEnsuredPage {

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            fb.Style.Add("margin-top", "7px");

            if (!Page.IsPostBack) {
                Exception ex = new Exception();
                if (!cms.businesslogic.packager.Settings.HasFileAccess(ref ex)) {
                    
                    fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                    fb.Text = "<strong>" + ui.Text("errors", "filePermissionsError") + ":</strong><br/>" + ex.Message;
                }
            }


        }
        protected void Page_Load(object sender, EventArgs e) {
            /*
            if (!Page.IsPostBack) {
                Exception ex = new Exception();
                if (!cms.businesslogic.packager.Settings.HasFileAccess(ref ex)) {
                    fb.Style.Add("margin-top", "7px");
                    fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                    fb.Text = "<strong>" + ui.Text("errors", "filePermissionsError") + ":</strong><br/>" + ex.Message;
                }
            }*/

            
            if (cms.businesslogic.packager.InstalledPackage.isPackageInstalled("ae41aad0-1c30-11dd-bd0b-0800200c9a66")) {
                 boostInstalled.Visible = true;
                 boostInstalled.Text = ui.Text("installer", "runwayInstalled");
                 boostNotInstalled.Visible = false;
                 nitroPanel.Controls.Add(new UserControl().LoadControl(SystemDirectories.Umbraco + "/developer/packages/LoadNitros.ascx"));
              } else {
                 boostInstalled.Visible = false;
                 boostNotInstalled.Visible = true;
                 boostNotInstalled.Text = ui.Text("install") + " Runway";
              }
        }

        protected override bool  OnBubbleEvent(object source, EventArgs args)
        {
            fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.success;
            fb.Text = "<strong>Modules installed successfully.</strong> Each module can be uninstalled under the 'installed packages' section </p> <p><a href='boost.aspx'>Install additional modules</a></p>";

			ClientTools.ReloadActionNode(true, true);

            boostInstalled.Visible = false;
            boostNotInstalled.Visible = false;

            
 	        return base.OnBubbleEvent(source, args);
        }

        protected void installBoost(object sender, EventArgs e) {

            string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";
            string packageGuid = "ae41aad0-1c30-11dd-bd0b-0800200c9a66";

            cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();
            cms.businesslogic.packager.repositories.Repository repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);

            if (repo.HasConnection()) {
                string tempFile = p.Import(repo.fetch(packageGuid));
                p.LoadConfig(tempFile);
                p.Install(tempFile, packageGuid, repoGuid);

                boostInstalled.Visible = true;
                boostNotInstalled.Visible = false;
            } else {
                fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>No connection to repository.</strong> Runway could not be installed as there was no connection to: '" + repo.RepositoryUrl + "'";
            }
        }        
    }
}
