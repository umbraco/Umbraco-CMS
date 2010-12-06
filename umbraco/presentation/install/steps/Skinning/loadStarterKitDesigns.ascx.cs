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
            Guid kitGuid = new Guid(((LinkButton)sender).CommandArgument);

            cms.businesslogic.packager.Installer installer = new cms.businesslogic.packager.Installer();

            if (repo.HasConnection())
            {
                cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();

                string tempFile = p.Import(repo.fetch(kitGuid.ToString()));
                p.LoadConfig(tempFile);
                int pID = p.CreateManifest(tempFile, kitGuid.ToString(), repoGuid);

                p.InstallFiles(pID, tempFile);
                p.InstallBusinessLogic(pID, tempFile);
                p.InstallCleanUp(pID, tempFile);

                library.RefreshContent();

                if (cms.businesslogic.skinning.Skinning.GetAllSkins().Count > 0)
                {
                    cms.businesslogic.skinning.Skinning.ActivateAsCurrentSkin(cms.businesslogic.skinning.Skinning.GetAllSkins()[0]);
                }

                try
                {
                    if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus))
                    {
                        GlobalSettings.ConfigurationStatus = GlobalSettings.CurrentVersion;
                        Application["umbracoNeedConfiguration"] = false;
                    }
                }
                catch{
                    _default pa = (_default)this.Page;
                    pa.GotoNextStep(helper.Request("installStep"));
                }
                
                _default page = (_default)this.Page;
                page.GotoNextStep(helper.Request("installStep"));
            }
            else
            {
                ShowConnectionError();
            }
        }
    }
}