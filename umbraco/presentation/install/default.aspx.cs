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

namespace umbraco.presentation.install
{
	/// <summary>
	/// Summary description for _default.
	/// </summary>
    public partial class _default : BasePages.BasePage
	{

		private string _installStep = "";

		protected void Page_Load(object sender, System.EventArgs e)
		{
			ClientLoader.DataBind();

            //If user wishes to subscribe to security updates
            if (!string.IsNullOrEmpty(Request["email"]) && !string.IsNullOrEmpty(Request["name"]))
                SubscribeToNewsLetter(Request["name"], Request["email"]);

            // use buffer, so content isn't sent until it's ready (minimizing the blank screen experience)
            Response.Buffer = true;
			step.Value = _installStep;
            //ScriptManager sm = Page.FindControl("umbracoScriptManager") as ScriptManager;
            //webservices.ajaxHelpers.EnsureLegacyCalls(Page);
            prepareNextButton();
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


		private void loadContent() 
		{
            //Response.Redirect("./default.aspx?installStep=" + step.Value, true);

		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
            InitializeComponent();
            base.OnInit(e);
            _installStep = helper.Request("installStep");
            
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

           	// empty / security check: only controls inside steps folder allowed
			if (_installStep == "" || _installStep.Contains("/"))
				_installStep = "welcome";

			PlaceHolderStep.Controls.Add(new System.Web.UI.UserControl().LoadControl( IOHelper.ResolveUrl( SystemDirectories.Install ) + "/steps/" + _installStep + ".ascx"));
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

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
					step.Value = "validatePermissions";
					loadContent();
					break;
				case "upgradeIndex":
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
		}
	}
}
