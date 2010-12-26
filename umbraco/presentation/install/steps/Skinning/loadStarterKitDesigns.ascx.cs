using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;

namespace umbraco.presentation.install.steps.Skinning
{
    public partial class loadStarterKitDesigns : System.Web.UI.UserControl
    {
        public Guid StarterKitGuid { get; set; }

        private cms.businesslogic.packager.repositories.Repository repo;
        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public loadStarterKitDesigns()
        {
            repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

       

        protected override void OnInit(EventArgs e)
        {
             base.OnInit(e);

            
             if (repo.HasConnection())
             {
                 try
                 {
                   //clear progress bar cache
                   Helper.clearProgress();

                     rep_starterKitDesigns.DataSource = repo.Webservice.Skins(StarterKitGuid.ToString());
                     rep_starterKitDesigns.DataBind();
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
            fb.Text = "<strong>No connection to repository.</strong> Starter Kits Designs could not be fetched from the repository as there was no connection to: '" + repo.RepositoryUrl + "'";

            pl_loadStarterKitDesigns.Controls.Clear();
            pl_loadStarterKitDesigns.Controls.Add(fb);
        }

        protected void SelectStarterKitDesign(object sender, EventArgs e)
        {
            Helper.clearProgress();

            Guid kitGuid = new Guid(((LinkButton)sender).CommandArgument);

            if (!cms.businesslogic.skinning.Skinning.IsSkinInstalled(kitGuid))
            {

                Helper.setProgress(5, "Fetching starting kit from the repository", "");

                cms.businesslogic.packager.Installer installer = new cms.businesslogic.packager.Installer();

                if (repo.HasConnection())
                {
                    cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();

                    Helper.setProgress(15, "Connected to repository", "");

                    string tempFile = p.Import(repo.fetch(kitGuid.ToString()));
                    p.LoadConfig(tempFile);
                    int pID = p.CreateManifest(tempFile, kitGuid.ToString(), repoGuid);

                    Helper.setProgress(30, "Installing skin files", "");
                    p.InstallFiles(pID, tempFile);

                    Helper.setProgress(50, "Installing skin system objects", "");
                    p.InstallBusinessLogic(pID, tempFile);

                    Helper.setProgress(60, "Finishing skin installation", "");
                    p.InstallCleanUp(pID, tempFile);

                    library.RefreshContent();

                    Helper.setProgress(80, "Activating skin", "");
                    if (cms.businesslogic.skinning.Skinning.GetAllSkins().Count > 0)
                    {
                        cms.businesslogic.skinning.Skinning.ActivateAsCurrentSkin(cms.businesslogic.skinning.Skinning.GetAllSkins()[0]);
                    }


                    Helper.setProgress(100, "Skin installation has been completed", "");

                    try
                    {


                        if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus))
                        {
                            GlobalSettings.ConfigurationStatus = GlobalSettings.CurrentVersion;
                            Application["umbracoNeedConfiguration"] = false;
                        }
                    }
                    catch
                    {
                        Helper.RedirectToNextStep(Page);
                    }

                    Helper.RedirectToNextStep(Page);
                }
                else
                {
                    ShowConnectionError();
                }

            }
        }
    }
}