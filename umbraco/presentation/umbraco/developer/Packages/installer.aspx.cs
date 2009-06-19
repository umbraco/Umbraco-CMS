using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.XPath;
using umbraco.BasePages;

namespace umbraco.presentation.developer.packages
{
	/// <summary>
	/// Summary description for packager.
	/// </summary>
	public partial class Installer : BasePages.UmbracoEnsuredPage
	{	
		private Control configControl;
        private cms.businesslogic.packager.repositories.Repository repo;	

		private cms.businesslogic.packager.Installer p = new cms.businesslogic.packager.Installer();
		private string tempFileName = "";

		protected void Page_Load(object sender, System.EventArgs e)
		{
            Exception ex = new Exception();
            if (!cms.businesslogic.packager.Settings.HasFileAccess(ref ex)) {
                fb.Style.Add("margin-top", "7px");
                fb.type = uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>" + ui.Text("errors", "filePermissionsError") + ":</strong><br/>" + ex.Message;
            }

            if (!IsPostBack)
            {
                ButtonInstall.Attributes.Add("onClick", "jQuery(this).hide(); jQuery('#installingMessage').show();; return true;");
                ButtonLoadPackage.Attributes.Add("onClick", "jQuery(this).hide(); jQuery('#loadingbar').show();; return true;");
            }

            //if we are actually in the middle of installing something... 
            if (!String.IsNullOrEmpty(helper.Request("installing"))) {
                hideAllPanes();
                pane_installing.Visible = true;
                processInstall(helper.Request("installing"));

            } else if (!String.IsNullOrEmpty(helper.Request("guid")) && !String.IsNullOrEmpty(helper.Request("repoGuid")))
            {
                //we'll fetch the local information we have about our repo, to find out what webservice to query.
                repo = cms.businesslogic.packager.repositories.Repository.getByGuid(helper.Request("repoGuid"));

                if (repo.HasConnection()) {
                    //from the webservice we'll fetch some info about the package.
                    cms.businesslogic.packager.repositories.Package pack = repo.Webservice.PackageByGuid(helper.Request("guid"));

                    //if the package is protected we will ask for the users credentials. (this happens every time they try to fetch anything)
                    if (!pack.Protected) {
                        //if it isn't then go straigt to the accept licens screen
                        tempFile.Value = p.Import(repo.fetch(helper.Request("guid")));
                        updateSettings();

                    } else if (!IsPostBack) {

                        //Authenticate against the repo
                        hideAllPanes();
                        pane_authenticate.Visible = true;

                    }
                } else {
                    fb.Style.Add("margin-top", "7px");
                    fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                    fb.Text = "<strong>No connection to repository.</strong> Runway could not be installed as there was no connection to: '" + repo.RepositoryUrl + "'";
                    pane_upload.Visible = false;
                }
            }

            
		}

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            acceptCheckbox.Attributes.Add("onmouseup", "document.getElementById('" + ButtonInstall.ClientID + "').disabled = false;");
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			if (helper.Request("config") != "") 
			{
				drawConfig();
			}
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

		protected void uploadFile(object sender, System.EventArgs e)
		{
            try {
                tempFileName = Guid.NewGuid().ToString() + ".umb";
                string fileName = GlobalSettings.StorageDirectory + System.IO.Path.DirectorySeparatorChar + tempFileName;
                file1.PostedFile.SaveAs(Server.MapPath(fileName));
                tempFile.Value = p.Import(tempFileName);
                updateSettings();
            } catch (Exception ex) {
                fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>Could not upload file</strong><br/>" + ex.ToString();
            }
		}

        //this fetches the protected package from the repo.
        protected void fetchProtectedPackage(object sender, EventArgs e) {
            //we auth against the webservice. This key will be used to fetch the protected package.
            string memberGuid = repo.Webservice.authenticate(tb_email.Text, library.md5(tb_password.Text));
            
            //if we auth correctly and get a valid key back, we will fetch the file from the repo webservice.
            if(!string.IsNullOrEmpty(memberGuid)){
                 tempFile.Value = p.Import(repo.fetch(helper.Request("guid"), memberGuid));
                 updateSettings();
            }
        }
       
        //this loads the accept license screen
        private void updateSettings()
        {
            hideAllPanes();

            pane_acceptLicense.Visible = true;
            pane_acceptLicenseInner.Text = "Installing the package: " + p.Name;
            Panel1.Text = "Installing the package: " + p.Name;


            if (p.ContainsUnsecureFiles && repo == null)
            {
                pp_unsecureFiles.Visible = true;
                foreach (string str in p.UnsecureFiles)
                {
                    lt_files.Text += "<li>" + str + "</li>";
                }
            }
            else
            {
                pp_unsecureFiles.Visible = false;
            }


            LabelName.Text = p.Name + " Version: " + p.Version;
            LabelMore.Text = "<a href=\"" + p.Url + "\" target=\"_blank\">" + p.Url + "</a>";
            LabelAuthor.Text = "<a href=\"" + p.AuthorUrl + "\" target=\"_blank\">" + p.Author + "</a>";
            LabelLicense.Text = "<a href=\"" + p.LicenseUrl + "\" target=\"_blank\">" + p.License + "</a>";
            
            if (p.ReadMe != "")
                readme.Text = "<div style=\"border: 1px solid #999; padding: 5px; overflow: auto; width: 370px; height: 160px;\">" + library.ReplaceLineBreaks(library.StripHtml(p.ReadMe)) + "</div>";
            else
                readme.Text = "<span style=\"color: #999\">No information</span><br/>";
        }


        private void processInstall(string currentStep) {
            string dir = helper.Request("dir");
            int packageId = 0;
            int.TryParse(helper.Request("pId"), out packageId);
            
            //first load in the config from the temporary directory
            //this will ensure that the installer have access to all the new files and the package manifest
            
            p.LoadConfig(dir);

            switch (currentStep) {
                case "businesslogic":
                    
                    p.InstallBusinessLogic(packageId, dir);


                    //making sure that publishing actions performed from the cms layer gets pushed to the presentation
                    library.RefreshContent();


                    if (p.Control != null && p.Control != "") {
                        Response.Redirect("installer.aspx?installing=customInstaller&dir=" + dir + "&pId=" + packageId.ToString());
                    } else {
                        Response.Redirect("installer.aspx?installing=finished&dir=" + dir + "&pId=" + packageId.ToString());
                    }
                    break;
                case "customInstaller":
                    if (p.Control != null && p.Control != "") {
                        hideAllPanes();

                        configControl = new System.Web.UI.UserControl().LoadControl(GlobalSettings.Path + "/.." + p.Control);
                        configControl.ID = "packagerConfigControl";

                        pane_optional.Controls.Add(configControl);
                        pane_optional.Visible = true;
                    } else {
                        hideAllPanes();
                        pane_success.Visible = true;
						BasePage.Current.ClientTools.ReloadActionNode(true, true);
                    }
                    break;
                case "finished":
                    hideAllPanes();
                    string url = p.Url;
                    string packageViewUrl = "installedPackage.aspx?id=" + packageId.ToString();

                    bt_viewInstalledPackage.OnClientClick = "document.location = '" + packageViewUrl + "'; return false;";
                    
                    if (!string.IsNullOrEmpty(url))
                        lit_authorUrl.Text = " <em>" + ui.Text("or") + "</em> <a href='" + url + "' target=\"_blank\">" + ui.Text("viewPackageWebsite") + "</a>";


                    pane_success.Visible = true;
					BasePage.Current.ClientTools.ReloadActionNode(true, true);

                    p.InstallCleanUp(packageId, dir);
                    break;
                default:
                    break;
            }
        }

        //this accepts the package, creates the manifest and then installs the files.
        protected void startInstall(object sender, System.EventArgs e)
		{
            //we will now create the installer manifest, which means that umbraco can register everything that gets added to the system
            //this returns an id of the manifest.

            p.LoadConfig(tempFile.Value);

            int pId = p.CreateManifest(tempFile.Value, helper.Request("guid"), helper.Request("repoGuid"));

            //and then copy over the files. This will take some time if it contains .dlls that will reboot the system..
            p.InstallFiles(pId, tempFile.Value);

            Response.Redirect("installer.aspx?installing=businesslogic&dir=" + tempFile.Value + "&pId=" + pId.ToString());
        }


		private void drawConfig() 
		{
            hideAllPanes();

			configControl = new System.Web.UI.UserControl().LoadControl(GlobalSettings.Path + "/.." + helper.Request("config"));
			configControl.ID = "packagerConfigControl";

			pane_optional.Controls.Add(configControl);
            pane_optional.Visible = true;
		}


        private void hideAllPanes() {
            pane_authenticate.Visible = false;
            pane_acceptLicense.Visible = false;
            pane_installing.Visible = false;
            pane_optional.Visible = false;
            pane_success.Visible = false;
            pane_upload.Visible = false;
        }
	}
}
