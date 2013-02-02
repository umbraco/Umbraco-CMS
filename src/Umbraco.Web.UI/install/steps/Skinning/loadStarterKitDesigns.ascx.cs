using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Web.Install;
using umbraco;
using GlobalSettings = global::Umbraco.Core.Configuration.GlobalSettings;

namespace Umbraco.Web.UI.Install.Steps.Skinning
{
    public delegate void StarterKitDesignInstalledEventHandler();

    public partial class LoadStarterKitDesigns : StepUserControl
	{

        public event StarterKitDesignInstalledEventHandler StarterKitDesignInstalled;

		protected virtual void OnStarterKitDesignInstalled()
		{
			if (StarterKitDesignInstalled != null)
				StarterKitDesignInstalled();
		}

		public Guid StarterKitGuid { get; set; }

		private readonly global::umbraco.cms.businesslogic.packager.repositories.Repository _repo;
	    private const string RepoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        public LoadStarterKitDesigns()
		{
            _repo = global::umbraco.cms.businesslogic.packager.repositories.Repository.getByGuid(RepoGuid);
		}
        
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (_repo.HasConnection())
			{
				try
				{
					//clear progress bar cache
					InstallHelper.ClearProgress();

					var skinsCollection = _repo.Webservice.Skins(StarterKitGuid.ToString());

					var numberOfSkins = skinsCollection.Length;
					this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "skinCounter", "var numberOfSkins = " + numberOfSkins, true);

					rep_starterKitDesigns.DataSource = skinsCollection;
					rep_starterKitDesigns.DataBind();
				}
				catch (Exception ex)
				{
                    LogHelper.Error<LoadStarterKitDesigns>("An error occurred initializing", ex);

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

			var fb = new global::umbraco.uicontrols.Feedback
			    {
			        type = global::umbraco.uicontrols.Feedback.feedbacktype.error, 
                    Text = "<strong>No connection to repository.</strong> Starter Kits Designs could not be fetched from the repository as there was no connection to: '" + _repo.RepositoryUrl + "'"
			    };

		    pl_loadStarterKitDesigns.Controls.Clear();
			pl_loadStarterKitDesigns.Controls.Add(fb);
		}

		protected void SelectStarterKitDesign(object sender, EventArgs e)
		{
            InstallHelper.ClearProgress();

			var kitGuid = new Guid(((LinkButton)sender).CommandArgument);

            if (!global::umbraco.cms.businesslogic.skinning.Skinning.IsSkinInstalled(kitGuid))
			{

				InstallHelper.SetProgress(5, "Fetching starting kit from the repository", "");

                var installer = new global::umbraco.cms.businesslogic.packager.Installer();

				if (_repo.HasConnection())
				{
                    var p = new global::umbraco.cms.businesslogic.packager.Installer();

                    InstallHelper.SetProgress(15, "Connected to repository", "");

					string tempFile = p.Import(_repo.fetch(kitGuid.ToString()));
					p.LoadConfig(tempFile);
					int pID = p.CreateManifest(tempFile, kitGuid.ToString(), RepoGuid);

                    InstallHelper.SetProgress(30, "Installing skin files", "");
					p.InstallFiles(pID, tempFile);

                    InstallHelper.SetProgress(50, "Installing skin system objects", "");
					p.InstallBusinessLogic(pID, tempFile);

                    InstallHelper.SetProgress(60, "Finishing skin installation", "");
					p.InstallCleanUp(pID, tempFile);

					library.RefreshContent();

                    InstallHelper.SetProgress(80, "Activating skin", "");
                    if (global::umbraco.cms.businesslogic.skinning.Skinning.GetAllSkins().Count > 0)
					{
                        global::umbraco.cms.businesslogic.skinning.Skinning.ActivateAsCurrentSkin(
                            global::umbraco.cms.businesslogic.skinning.Skinning.GetAllSkins()[0]);
					}


                    InstallHelper.SetProgress(100, "Skin installation has been completed", "");

					try
					{


						if (string.IsNullOrEmpty(GlobalSettings.ConfigurationStatus))
						{
                            GlobalSettings.ConfigurationStatus = UmbracoVersion.Current.ToString(3);
							Application["umbracoNeedConfiguration"] = false;
						}
					}
					catch
					{

					}

					try
					{
						InstallHelper.RedirectToNextStep(Page, GetCurrentStep());
					}
					catch
					{
						OnStarterKitDesignInstalled();
					}
				}
				else
				{
					ShowConnectionError();
				}

			}
		}
	}
}