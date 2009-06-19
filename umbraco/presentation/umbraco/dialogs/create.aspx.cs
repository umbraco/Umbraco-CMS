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

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for create.
	/// </summary>
	public partial class create : BasePages.UmbracoEnsuredPage
	{
		protected System.Web.UI.WebControls.Button ok;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			if (helper.Request("nodeId") == "") 
			{
				// Caption and properies on BUTTON
//				ok.Text = ui.Text("general", "continue", this.getUser());
//				ok.Attributes.Add("style", "width: 100px");
//				ok.Attributes.Add("disabled", "true");
				string appType = ui.Text("sections", helper.Request("app")).ToLower();
                pane_chooseNode.Text = ui.Text("create", "chooseNode", appType, this.getUser()) + "?";
			} 
			else 
			{
                //pane_chooseName.Text = ui.Text("create", "updateData", this.getUser());
				cms.businesslogic.CMSNode c = new cms.businesslogic.CMSNode(int.Parse(helper.Request("nodeId")));
				path.Value = c.Path;
				pane_chooseNode.Visible = false;
                panel_buttons.Visible = false;
                pane_chooseName.Visible = true;
				XmlDocument createDef = new XmlDocument();
				XmlTextReader defReader = new XmlTextReader(Server.MapPath(GlobalSettings.Path+"/config/create/UI.xml"));
				createDef.Load(defReader);
				defReader.Close();

				// Find definition for current nodeType
				XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + Request.QueryString["app"] + "']");
				phCreate.Controls.Add(new UserControl().LoadControl(GlobalSettings.Path+ def.SelectSingleNode("./usercontrol").FirstChild.Value));
			}
		}

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/cmsnode.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
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
	}
}
