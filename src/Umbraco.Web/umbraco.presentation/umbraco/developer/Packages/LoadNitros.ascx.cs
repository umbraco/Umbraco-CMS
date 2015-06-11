using System.IO;
using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;

namespace umbraco.presentation.developer.packages {
    public partial class LoadNitros : System.Web.UI.UserControl {

        private List<CheckBox> _nitroList = new List<CheckBox>();
        private List<string> _selectedNitros = new List<string>();

        protected void Page_Load(object sender, EventArgs e) { }

        public void installNitros(object sender, EventArgs e) {

            string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66"; //Hardcoded official package repo key.

            var p = new cms.businesslogic.packager.Installer(Umbraco.Web.UmbracoContext.Current.Security.CurrentUser.Id);
            var repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);

            if (repo == null)
            {
                throw new InvalidOperationException("Could not find repository with id " + repoGuid);
            }

            foreach (CheckBox cb in _nitroList) {
                if (cb.Checked) {
                    if (!_selectedNitros.Contains(cb.ID))
                        _selectedNitros.Add(cb.ID);
                }
            }

            foreach (string guid in _selectedNitros) {

                string tempFile = p.Import(repo.fetch(guid));
                p.LoadConfig(tempFile);

                int pId = p.CreateManifest(tempFile, guid, repoGuid);

                //and then copy over the files. This will take some time if it contains .dlls that will reboot the system..
                p.InstallFiles(pId, tempFile);

                //finally install the businesslogic
                p.InstallBusinessLogic(pId, tempFile);

                //cleanup.. 
                p.InstallCleanUp(pId, tempFile);

            }

            ApplicationContext.Current.ApplicationCache.ClearAllCache();
            library.RefreshContent();

            loadNitros.Visible = false;

            RaiseBubbleEvent(new object(), new EventArgs());
        }

        protected void onCategoryDataBound(object sender, RepeaterItemEventArgs e) {

            if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item) {

                cms.businesslogic.packager.repositories.Category cat = (cms.businesslogic.packager.repositories.Category)e.Item.DataItem;
                Literal _name = (Literal)e.Item.FindControl("lit_name");
                Literal _desc = (Literal)e.Item.FindControl("lit_desc");
                PlaceHolder _nitros = (PlaceHolder)e.Item.FindControl("ph_nitroHolder");

                _name.Text = cat.Text;
                _desc.Text = cat.Description;

                e.Item.Visible = false;

                foreach (cms.businesslogic.packager.repositories.Package nitro in cat.Packages) {
                    bool installed = cms.businesslogic.packager.InstalledPackage.isPackageInstalled(nitro.RepoGuid.ToString());
                    int localPackageID = 0;

                    if (installed)
                        localPackageID = cms.businesslogic.packager.InstalledPackage.GetByGuid(nitro.RepoGuid.ToString()).Data.Id;

                    CheckBox cb_nitro = new CheckBox();
                    cb_nitro.ID = nitro.RepoGuid.ToString();
                    cb_nitro.Enabled = !installed;

                    cb_nitro.CssClass = "nitroCB";                   

                    cb_nitro.Text = "<div class='nitro'><h3>" + nitro.Text;

                    if (installed) {
                        cb_nitro.CssClass = "nitroCB installed";
                        cb_nitro.Text += "<span><a href='#' onclick=\"openDemoModal('" + nitro.RepoGuid.ToString() + "','" + nitro.Text + "'); return false;\">Already installed</a></span>";
                    }
                    
                    cb_nitro.Text += "</h3><small>" + nitro.Description + "<br/>";

                    if (!string.IsNullOrEmpty(nitro.Demo)) {
                        cb_nitro.Text += "<a href='#' onclick=\"openDemoModal('" + nitro.RepoGuid.ToString() + "','" + nitro.Text + "'); return false;\">Demonstration</a> &nbsp;";
                    }

                    if (!string.IsNullOrEmpty(nitro.Documentation)) {
                        cb_nitro.Text += "<a href='" + nitro.Documentation + "' target='_blank'>Documentation</a> &nbsp;";
                    }

                    cb_nitro.Text += "</small></div><br style='clear: both'/>";

                    _nitros.Controls.Add(cb_nitro);
                    _nitroList.Add(cb_nitro);

                    e.Item.Visible = true;

                    if (nitro.EditorsPick) {

                        CheckBox cb_Recnitro = new CheckBox();
                        cb_Recnitro.ID = nitro.RepoGuid.ToString();
                        cb_Recnitro.CssClass = "nitroCB";
                        cb_Recnitro.Enabled = !installed;

                        cb_Recnitro.Text = "<div class='nitro'><h3>" + nitro.Text;

                        if (installed) {
                            cb_Recnitro.CssClass = "nitroCB installed";
                            cb_Recnitro.Text += "<span><a href='' onclick=\"openDemoModal('" + nitro.RepoGuid.ToString() + "','" + nitro.Text + "'); return false;\">Already installed</a></span>";
                        }

                        cb_Recnitro.Text += "</h3><small>" + nitro.Description + "<br/>";

                        if (!string.IsNullOrEmpty(nitro.Demo)) {
                            cb_Recnitro.Text += "<a href='#' onclick=\"openDemoModal('" + nitro.RepoGuid.ToString() + "','" + nitro.Text + "'); return false;\">Demonstration</a> &nbsp;";
                        }

                        if (!string.IsNullOrEmpty(nitro.Documentation)) {
                            cb_Recnitro.Text += "<a href='" + nitro.Documentation + "' target='_blank'>Documentation</a> &nbsp;";
                        }

                        cb_Recnitro.Text += "</small></div><br style='clear: both'/>";


                        _nitroList.Add(cb_Recnitro);
                        ph_recommendedHolder.Controls.Add(cb_Recnitro);

                    }
                }

            }
        }


        protected override void OnInit(EventArgs e) {

            base.OnInit(e);

            string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";
            var repo = cms.businesslogic.packager.repositories.Repository.getByGuid(repoGuid);

            if (repo == null)
            {
                throw new InvalidOperationException("Could not find repository with id " + repoGuid);
            }

            var fb = new global::umbraco.uicontrols.Feedback();
            fb.type = global::umbraco.uicontrols.Feedback.feedbacktype.error;
            fb.Text = "<strong>No connection to repository.</strong> Modules could not be fetched from the repository as there was no connection to: '" + repo.RepositoryUrl + "'";


			if (repo.HasConnection())
			{
				try
				{

					if (UmbracoConfig.For.UmbracoSettings().Content.UseLegacyXmlSchema)
						rep_nitros.DataSource = repo.Webservice.NitrosCategorizedByVersion(cms.businesslogic.packager.repositories.Version.Version4);
					else
						rep_nitros.DataSource = repo.Webservice.NitrosCategorizedByVersion(cms.businesslogic.packager.repositories.Version.Version41);

					rep_nitros.DataBind();
				}
				catch (Exception ex)
				{
					LogHelper.Error<LoadNitros>("An error occurred", ex);

					loadNitros.Controls.Clear();
					loadNitros.Controls.Add(fb);
					//nitroList.Visible = false;
					//lt_status.Text = "<div class='error'><p>Nitros could not be fetched from the repository. Please check your internet connection</p><p>You can always install Nitros later in the packages section</p><p>" + ex.ToString() + "</p></div>";
				}
			}
			else
			{
				loadNitros.Controls.Clear();
				loadNitros.Controls.Add(fb);
			}
        }
    }
}