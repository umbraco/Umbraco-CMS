using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.IO;
using System.IO;

namespace umbraco.presentation.install.steps.Skinning
{
    public partial class loadStarterKits : System.Web.UI.UserControl
    {
        private cms.businesslogic.packager.repositories.Repository repo;
        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public loadStarterKits()
        {
            repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void NextStep(object sender, EventArgs e)
        {
            _default p = (_default)this.Page;
            p.GotoNextStep(helper.Request("installStep"));
        }

        protected override void OnInit(EventArgs e)
        {
                base.OnInit(e);

          //clear progressbar cache
                Helper.clearProgress();

                if (repo.HasConnection())
                {
                    try
                    {                        
                        rep_starterKits.DataSource = repo.Webservice.StarterKits();
                        rep_starterKits.DataBind(); 
                    }
                    catch (Exception ex)
                    {
                        Log.Add(LogTypes.Debug, -1, ex.ToString());
                        ShowConnectionError();
                       
                    }
                }
                else
                {
                    ShowConnectionError();
                }
        }

        private void ShowConnectionError()
        {

            uicontrols.Feedback fb = new global::umbraco.uicontrols.Feedback();
            fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
            fb.Text = "<strong>No connection to repository.</strong> Starter Kits could not be fetched from the repository as there was no connection to: '" + repo.RepositoryUrl + "'";
            
            pl_loadStarterKits.Controls.Clear();
            pl_loadStarterKits.Controls.Add(fb);
        }

        protected void SelectStarterKit(object sender, EventArgs e)
        {
            

            Guid kitGuid = new Guid(((LinkButton)sender).CommandArgument);

            Helper.setProgress(10, "Connecting to skin repository", "");

            cms.businesslogic.packager.Installer installer = new cms.businesslogic.packager.Installer();

            if (repo.HasConnection())
            {

                Helper.setProgress(20, "Downloading skin files...", "");

                cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();

                string tempFile = p.Import(repo.fetch(kitGuid.ToString()));
                p.LoadConfig(tempFile);
                int pID = p.CreateManifest(tempFile, kitGuid.ToString(), repoGuid);

                Helper.setProgress(40, "Installing skin files", "");
                p.InstallFiles(pID, tempFile);

                Helper.setProgress(60, "Installing skin system objects", "");
                p.InstallBusinessLogic(pID, tempFile);

                Helper.setProgress(80, "Cleaning up after installation", "");
                p.InstallCleanUp(pID, tempFile);

                library.RefreshContent();

                Helper.setProgress(100, "Skin has been installed", "");
                ((skinning)Parent.Parent.Parent.Parent.Parent).showStarterKitDesigns(kitGuid);

            }
            else
            {
                ShowConnectionError();
            }        


        }
    }
}