using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.BusinessLogic;

namespace umbraco.presentation.umbraco.developer.Packages
{
	public partial class StarterKits : UmbracoEnsuredPage
	{
	    public StarterKits()
	    {
	        CurrentApp = DefaultApps.developer.ToString();
	    }

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
				ShowStarterKits();
			else
				showSkins((Guid)cms.businesslogic.skinning.Skinning.StarterKitGuid());
		}

		private void ShowStarterKits()
		{
			var starterkitsctrl =
				(install.steps.Skinning.loadStarterKits)LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKits.ascx");
			starterkitsctrl.StarterKitInstalled += starterkitsctrl_StarterKitInstalled;

			ph_starterkits.Controls.Add(starterkitsctrl);



			StarterKitNotInstalled.Visible = true;
			StarterKitInstalled.Visible = false;

		}



		public void showSkins(Guid starterKitGuid)
		{

			var ctrl = (install.steps.Skinning.loadStarterKitDesigns)new UserControl().LoadControl(SystemDirectories.Install + "/steps/Skinning/loadStarterKitDesigns.ascx");
			ctrl.ID = "StarterKitDesigns";

			ctrl.StarterKitGuid = starterKitGuid;
			ctrl.StarterKitDesignInstalled += ctrl_StarterKitDesignInstalled;
			ph_skins.Controls.Add(ctrl);

			StarterKitNotInstalled.Visible = false;
			StarterKitInstalled.Visible = true;

		}

		void starterkitsctrl_StarterKitInstalled()
		{
			StarterKitNotInstalled.Visible = false;
			StarterKitInstalled.Visible = false;

			installationCompleted.Visible = true;

		}

		void ctrl_StarterKitDesignInstalled()
		{
			StarterKitNotInstalled.Visible = false;
			StarterKitInstalled.Visible = false;

			installationCompleted.Visible = true;
		}

		/// <summary>
		/// JsInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

		/// <summary>
		/// Panel1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoPanel Panel1;

		/// <summary>
		/// fb control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Feedback fb;

		/// <summary>
		/// StarterKitInstalled control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane StarterKitInstalled;

		/// <summary>
		/// ph_skins control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.PlaceHolder ph_skins;

		/// <summary>
		/// StarterKitNotInstalled control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane StarterKitNotInstalled;

		/// <summary>
		/// ph_starterkits control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.PlaceHolder ph_starterkits;

		/// <summary>
		/// installationCompleted control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane installationCompleted;
	}
}