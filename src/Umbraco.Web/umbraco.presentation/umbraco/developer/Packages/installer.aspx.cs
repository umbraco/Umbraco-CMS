using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Core.IO;
using Umbraco.Web;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using BizLogicAction = umbraco.BusinessLogic.Actions.Action;

namespace umbraco.presentation.developer.packages
{
    /// <summary>
    /// Summary description for packager.
    /// </summary>
    public partial class Installer : UmbracoEnsuredPage
    {
        public Installer()
        {
            CurrentApp = DefaultApps.developer.ToString();
        }

        private Control _configControl;
        private cms.businesslogic.packager.repositories.Repository _repo;
        private readonly cms.businesslogic.packager.Installer _installer = new cms.businesslogic.packager.Installer();
        private string _tempFileName = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            var ex = new Exception();
            if (!cms.businesslogic.packager.Settings.HasFileAccess(ref ex))
            {
                fb.Style.Add("margin-top", "7px");
                fb.type = uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>" + ui.Text("errors", "filePermissionsError") + ":</strong><br/>" + ex.Message;
            }

            if (!IsPostBack)
            {
                ButtonInstall.Attributes.Add("onClick", "jQuery(this).hide(); jQuery('#installingMessage').show();; return true;");
                ButtonLoadPackage.Attributes.Add("onClick", "jQuery(this).hide(); jQuery('#loadingbar').show();; return true;");
            }

            //if we are actually in the middle of installing something... meaning we keep redirecting back to this page with 
 			// custom query strings 
			// TODO: SD: This process needs to be fixed/changed/etc... to use the InstallPackageController
			//	http://issues.umbraco.org/issue/U4-1047
            if (!string.IsNullOrEmpty(Request.GetItemAsString("installing")))
            {
                HideAllPanes();
                pane_installing.Visible = true;
                ProcessInstall(Request.GetItemAsString("installing")); //process the current step

            }
			else if (tempFile.Value.IsNullOrWhiteSpace() //if we haven't downloaded the .umb temp file yet
				&& (!Request.GetItemAsString("guid").IsNullOrWhiteSpace() && !Request.GetItemAsString("repoGuid").IsNullOrWhiteSpace()))
            {
                //we'll fetch the local information we have about our repo, to find out what webservice to query.
				_repo = cms.businesslogic.packager.repositories.Repository.getByGuid(Request.GetItemAsString("repoGuid"));

                if (_repo.HasConnection())
                {
                    //from the webservice we'll fetch some info about the package.
					cms.businesslogic.packager.repositories.Package pack = _repo.Webservice.PackageByGuid(Request.GetItemAsString("guid"));

                    //if the package is protected we will ask for the users credentials. (this happens every time they try to fetch anything)
                    if (!pack.Protected)
                    {
                        //if it isn't then go straigt to the accept licens screen
						tempFile.Value = _installer.Import(_repo.fetch(Request.GetItemAsString("guid")));
                        UpdateSettings();

                    }
                    else if (!IsPostBack)
                    {

                        //Authenticate against the repo
                        HideAllPanes();
                        pane_authenticate.Visible = true;

                    }
                }
                else
                {
                    fb.Style.Add("margin-top", "7px");
                    fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                    fb.Text = "<strong>No connection to repository.</strong> Runway could not be installed as there was no connection to: '" + _repo.RepositoryUrl + "'";
                    pane_upload.Visible = false;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            acceptCheckbox.Attributes.Add("onmouseup", "document.getElementById('" + ButtonInstall.ClientID + "').disabled = false;");
        }

        protected void uploadFile(object sender, System.EventArgs e)
        {
            try
            {
                _tempFileName = Guid.NewGuid().ToString() + ".umb";
                string fileName = SystemDirectories.Data + System.IO.Path.DirectorySeparatorChar + _tempFileName;
                file1.PostedFile.SaveAs(IOHelper.MapPath(fileName));
                tempFile.Value = _installer.Import(_tempFileName);
                UpdateSettings();
            }
            catch (Exception ex)
            {
                fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
                fb.Text = "<strong>Could not upload file</strong><br/>" + ex.ToString();
            }
        }

        //this fetches the protected package from the repo.
        protected void fetchProtectedPackage(object sender, EventArgs e)
        {
            //we auth against the webservice. This key will be used to fetch the protected package.
            string memberGuid = _repo.Webservice.authenticate(tb_email.Text, library.md5(tb_password.Text));

            //if we auth correctly and get a valid key back, we will fetch the file from the repo webservice.
            if (!string.IsNullOrEmpty(memberGuid))
            {
                tempFile.Value = _installer.Import(_repo.fetch(helper.Request("guid"), memberGuid));
                UpdateSettings();
            }
        }

        //this loads the accept license screen
        private void UpdateSettings()
        {
            HideAllPanes();

            pane_acceptLicense.Visible = true;
            pane_acceptLicenseInner.Text = "Installing the package: " + _installer.Name;
            Panel1.Text = "Installing the package: " + _installer.Name;


            if (_installer.ContainsUnsecureFiles && _repo == null)
            {
                pp_unsecureFiles.Visible = true;
                foreach (string str in _installer.UnsecureFiles)
                {
                    lt_files.Text += "<li>" + str + "</li>";
                }
            }

            if (_installer.ContainsMacroConflict)
            {
                pp_macroConflicts.Visible = true;
                foreach (var item in _installer.ConflictingMacroAliases)
                {
                    ltrMacroAlias.Text += "<li>" + item.Key + " (Alias: " + item.Value + ")</li>";
                }
            }

            if (_installer.ContainsTemplateConflicts)
            {
                pp_templateConflicts.Visible = true;
                foreach (var item in _installer.ConflictingTemplateAliases)
                {
                    ltrTemplateAlias.Text += "<li>" + item.Key + " (Alias: " + item.Value + ")</li>";
                }
            }

            if (_installer.ContainsStyleSheeConflicts)
            {
                pp_stylesheetConflicts.Visible = true;
                foreach (var item in _installer.ConflictingStyleSheetNames)
                {
                    ltrStylesheetNames.Text += "<li>" + item.Key + " (Alias: " + item.Value + ")</li>";
                }
            }

            LabelName.Text = _installer.Name + " Version: " + _installer.Version;
            LabelMore.Text = "<a href=\"" + _installer.Url + "\" target=\"_blank\">" + _installer.Url + "</a>";
            LabelAuthor.Text = "<a href=\"" + _installer.AuthorUrl + "\" target=\"_blank\">" + _installer.Author + "</a>";
            LabelLicense.Text = "<a href=\"" + _installer.LicenseUrl + "\" target=\"_blank\">" + _installer.License + "</a>";

            if (_installer.ReadMe != "")
            {
                // Strip all tags except for pre tags
                var strippedReadme = Regex.Replace(_installer.ReadMe, @"</?(?(?=pre)notag|[a-zA-Z0-9]+)(?:\s[a-zA-Z0-9\-]+=?(?:(["",']?).*?\1?)?)*\s*/?>", string.Empty);

                // Replace line breaks with a break tag except when inside a pre tag
                var breakTag = bool.Parse(Umbraco.Core.Configuration.GlobalSettings.EditXhtmlMode) ? "<br/>\n" : "<br />\n";
                strippedReadme = Regex.Replace(strippedReadme, "\n(?![^<]*</pre>)", breakTag);

                readme.Text = "<div style=\"border: 1px solid #999; padding: 5px; overflow: auto; width: 370px; height: 160px;\">" + strippedReadme + "</div>";
            }
            else
                readme.Text = "<span style=\"color: #999\">No information</span><br/>";
        }


        private void ProcessInstall(string currentStep)
        {
			var dir = Request.GetItemAsString("dir");
            var packageId = 0;
			int.TryParse(Request.GetItemAsString("pId"), out packageId);

            switch (currentStep)
            {
                case "businesslogic":
					//first load in the config from the temporary directory
					//this will ensure that the installer have access to all the new files and the package manifest
					_installer.LoadConfig(dir);
                    _installer.InstallBusinessLogic(packageId, dir);


                    //making sure that publishing actions performed from the cms layer gets pushed to the presentation
                    library.RefreshContent();
					
                    if (!string.IsNullOrEmpty(_installer.Control))
                    {
						Response.Redirect("installer.aspx?installing=customInstaller&dir=" + dir + "&pId=" + packageId.ToString() + "&customControl=" + Server.UrlEncode(_installer.Control) + "&customUrl=" + Server.UrlEncode(_installer.Url));
                    }
                    else
                    {
                        Response.Redirect("installer.aspx?installing=finished&dir=" + dir + "&pId=" + packageId.ToString() + "&customUrl=" + Server.UrlEncode(_installer.Url));
                    }
                    break;
                case "customInstaller":
		            var customControl = Request.GetItemAsString("customControl");

					if (!customControl.IsNullOrWhiteSpace())
                    {
                        HideAllPanes();

						_configControl = new System.Web.UI.UserControl().LoadControl(SystemDirectories.Root + customControl);
                        _configControl.ID = "packagerConfigControl";

                        pane_optional.Controls.Add(_configControl);
                        pane_optional.Visible = true;

						if (!IsPostBack)
						{
							//We still need to clean everything up which is normally done in the Finished Action
							PerformPostInstallCleanup(packageId, dir);	
						}
						
                    }
                    else
                    {
                        //if the custom installer control is empty here (though it should never be because we've already checked for it previously)
						//then we should run the normal FinishedAction
						PerformFinishedAction(packageId, dir, Request.GetItemAsString("customUrl"));
                    }
                    break;
                case "finished":
					PerformFinishedAction(packageId, dir, Request.GetItemAsString("customUrl"));
                    break;
                default:
                    break;
            }
        }

	    /// <summary>
	    /// Perform the 'Finished' action of the installer
	    /// </summary>
	    /// <param name="packageId"></param>
	    /// <param name="dir"></param>
	    /// <param name="url"></param>
	    private void PerformFinishedAction(int packageId, string dir, string url)
		{
			HideAllPanes();
			//string url = _installer.Url;
            string packageViewUrl = "installedPackage.aspx?id=" + packageId.ToString();

            bt_viewInstalledPackage.OnClientClick = "document.location = '" + packageViewUrl + "'; return false;";

            if (!string.IsNullOrEmpty(url))
                lit_authorUrl.Text = " <em>" + ui.Text("or") + "</em> <a href='" + url + "' target=\"_blank\">" + ui.Text("viewPackageWebsite") + "</a>";


            pane_success.Visible = true;

			PerformPostInstallCleanup(packageId, dir);
		}

		/// <summary>
		/// Runs Post install actions such as clearning any necessary cache, reloading the correct tree nodes, etc...
		/// </summary>
		/// <param name="packageId"></param>
		/// <param name="dir"></param>
		private void PerformPostInstallCleanup(int packageId, string dir)
		{
			BasePage.Current.ClientTools.ReloadActionNode(true, true);
			_installer.InstallCleanUp(packageId, dir);
			//clear the tree cache
			ClientTools.ClearClientTreeCache().RefreshTree("packager");
			TreeDefinitionCollection.Instance.ReRegisterTrees();
			BizLogicAction.ReRegisterActionsAndHandlers();
		}

        //this accepts the package, creates the manifest and then installs the files.
        protected void startInstall(object sender, System.EventArgs e)
        {
            //we will now create the installer manifest, which means that umbraco can register everything that gets added to the system
            //this returns an id of the manifest.

            _installer.LoadConfig(tempFile.Value);

            int pId = _installer.CreateManifest(tempFile.Value, helper.Request("guid"), helper.Request("repoGuid"));

            //and then copy over the files. This will take some time if it contains .dlls that will reboot the system..
            _installer.InstallFiles(pId, tempFile.Value);

			//TODO: This is a total hack, we need to refactor the installer to be just like the package installer during the 
			// install process and use AJAX to ensure that app pool restarts and restarts PROPERLY before installing the business
			// logic. Until then, we are going to put a thread sleep here for 2 seconds in hopes that we always fluke out and the app 
			// pool will be restarted after redirect.
	        Thread.Sleep(2000);

            Response.Redirect("installer.aspx?installing=businesslogic&dir=" + tempFile.Value + "&pId=" + pId.ToString());
        }

        private void HideAllPanes()
        {
            pane_authenticate.Visible = false;
            pane_acceptLicense.Visible = false;
            pane_installing.Visible = false;
            pane_optional.Visible = false;
            pane_success.Visible = false;
            pane_upload.Visible = false;
        }
    }
}
