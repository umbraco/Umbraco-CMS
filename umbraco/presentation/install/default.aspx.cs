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
using System.Collections.Specialized;
using umbraco.IO;
using umbraco.cms.businesslogic.installer;

namespace umbraco.presentation.install
{
	/// <summary>
	/// Summary description for _default.
	/// </summary>
	public partial class _default : BasePages.BasePage
	{

		private string _installStep = "";
        public string currentStepClass = "";

		protected void Page_Load(object sender, System.EventArgs e)
		{
			ClientLoader.DataBind();

			//If user wishes to subscribe to security updates
			if (!string.IsNullOrEmpty(Request["email"]) && !string.IsNullOrEmpty(Request["name"]))
				SubscribeToNewsLetter(Request["name"], Request["email"]);

			// use buffer, so content isn't sent until it's ready (minimizing the blank screen experience)
			Response.Buffer = true;
			
			//ScriptManager sm = Page.FindControl("umbracoScriptManager") as ScriptManager;
			//webservices.ajaxHelpers.EnsureLegacyCalls(Page);
			//prepareNextButton();
		}

		private void SubscribeToNewsLetter(string name, string email) {
			try {
				System.Net.WebClient client = new System.Net.WebClient();
				NameValueCollection values = new NameValueCollection();
				values.Add("name", name);
				values.Add("email", email);

				client.UploadValues("http://umbraco.org/base/Ecom/SubmitEmail/installer.aspx", values);

			} catch { /* fail in silence */ }
		}


		private void loadContent(InstallerStep currentStep) 
		{
			PlaceHolderStep.Controls.Clear();
			PlaceHolderStep.Controls.Add(new System.Web.UI.UserControl().LoadControl(IOHelper.ResolveUrl( currentStep.UserControl ) ));
			step.Value = currentStep.Alias;

            if (!currentStep.Completed() && currentStep.HideNextButtonUntillCompleted)
                next.Visible = false;
            else
            {
                next.Visible = true;
                next.CommandArgument = currentStep.Alias;
                next.Text = currentStep.NextButtonText;
                next.OnClientClick = currentStep.NextButtonClientSideClick;    
            }
            
			lt_header.Text = currentStep.Name;
            currentStepClass = currentStep.Alias;
		}


		protected void onNextCommand(object sender, CommandEventArgs e)
		{
            string currentStep = (string)e.CommandArgument;
            GotoNextStep(currentStep);
		}

        public void GotoNextStep(string currentStep)
        {
            InstallerStep _s = InstallerSteps().GotoNextStep(currentStep);
            Response.Redirect("?installStep=" + _s.Alias);
        }


		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();
			base.OnInit(e);

			//we might override the beginning step here
			_installStep = helper.Request("installStep");
		  
			InstallerStep currentStep;
			//if this is not an upgrade we will log in with the default user.
			
			if (!String.IsNullOrEmpty(GlobalSettings.ConfigurationStatus.Trim())) {
				try {
					ensureContext();
				} catch {
					Response.Redirect(SystemDirectories.Umbraco + "/logout.aspx?redir=" + Server.UrlEncode(Request.RawUrl));
				}

				//set the first step to upgrade.
				if (string.IsNullOrEmpty(_installStep))
					_installStep = "upgrade";

			}

			if (!string.IsNullOrEmpty(_installStep) && InstallerSteps().StepExists(_installStep))
				currentStep = InstallerSteps().Get(_installStep);
			else
				currentStep = InstallerSteps().FirstAvailableStep();

			//if the step we are loading is complete, we will continue to the next one if it's set to auto redirect
			if (currentStep.Completed() && currentStep.MoveToNextStepAutomaticly)
				currentStep = InstallerSteps().FirstAvailableStep();

			loadContent(currentStep);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion


		private static InstallerStepCollection InstallerSteps()
		{
			InstallerStepCollection ics = new InstallerStepCollection();
			ics.Add(new install.steps.Definitions.License());
			ics.Add(new install.steps.Definitions.FilePermissions());
			ics.Add(new install.steps.Definitions.Database());
            ics.Add(new install.steps.Definitions.DefaultUser());
            ics.Add( new install.steps.Definitions.Skinning() );
            ics.Add(new install.steps.Definitions.WebPi());
            ics.Add(new install.steps.Definitions.TheEnd());
			return ics;
		}
/*
		protected void prepareNextButton()
		{
			switch (step.Value) 
			{
				case "welcome": case "upgrade":
					step.Value = "license";
					loadContent();
					break;
				case "license":
					step.Value = "detect";
					loadContent();
					break;
				case "detect":
					// upgrade!
					if (!String.IsNullOrEmpty(GlobalSettings.ConfigurationStatus.Trim()))
						step.Value = "renaming";
					else
						step.Value = "validatePermissions";
					loadContent();
					break;
				case "upgradeIndex":
					step.Value = "validatePermissions";
					loadContent();
					break;
				case "renaming" :
					step.Value = "validatePermissions";
					loadContent();
					break;
				case "validatePermissions":
					step.Value = "defaultUser";
					loadContent();
					break;
				case "defaultUser":
					step.Value = "boost";
					loadContent();
					break;
//				case "theend":
//					Response.Redirect("http://umbraco.org/redir/getting-started", true);
//					break;
				default:
					break;
			}
		}*/ 
	}
}
