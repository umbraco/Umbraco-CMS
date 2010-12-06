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
using System.Collections.Generic;

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
      	  // use buffer, so content isn't sent until it's ready (minimizing the blank screen experience)
		  //Response.Buffer = true;
          rp_steps.DataSource = InstallerSteps().Values;
          rp_steps.DataBind();
		}

		
		private void loadContent(InstallerStep currentStep) 
		{
			PlaceHolderStep.Controls.Clear();
			PlaceHolderStep.Controls.Add(new System.Web.UI.UserControl().LoadControl(IOHelper.ResolveUrl( currentStep.UserControl ) ));
			step.Value = currentStep.Alias;
            currentStepClass = currentStep.Alias;
		}

    int stepCounter = 0;
    protected void bindStep(object sender, RepeaterItemEventArgs e) {

      if (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item) {
        InstallerStep i = (InstallerStep)e.Item.DataItem;

        if (!i.HideFromNavigation) {
          Literal _class = (Literal)e.Item.FindControl("lt_class");
          Literal _name = (Literal)e.Item.FindControl("lt_name");

          if (i.Alias == currentStepClass)
            _class.Text = "active";

          stepCounter++;
          _name.Text = (stepCounter).ToString() + " - " + i.Name;
        } else
          e.Item.Visible = false;

      }
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

            _installStep = helper.Request("installStep");
            InstallerStep _s;

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

            if (string.IsNullOrEmpty(_installStep))
                _s = InstallerSteps()["welcome"];
            else
                _s = InstallerSteps()[_installStep];

            loadContent(_s);            
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
			ics.Add(new install.steps.Definitions.Welcome());
            ics.Add(new install.steps.Definitions.License());
			ics.Add(new install.steps.Definitions.FilePermissions());
			ics.Add(new install.steps.Definitions.Database());
            ics.Add(new install.steps.Definitions.DefaultUser());
            ics.Add( new install.steps.Definitions.Skinning() );
            ics.Add(new install.steps.Definitions.WebPi());
            ics.Add(new install.steps.Definitions.TheEnd());
			return ics;
		}

		
	}
}
