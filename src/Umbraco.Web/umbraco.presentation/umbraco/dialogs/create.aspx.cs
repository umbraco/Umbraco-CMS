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
using Umbraco.Core.IO;
using umbraco.cms.businesslogic;
using umbraco.presentation;
using umbraco.BusinessLogic.Actions;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;

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
			
				string appType = ui.Text("sections", helper.Request("app")).ToLower();
                pane_chooseNode.Text = ui.Text("create", "chooseNode", appType, this.getUser()) + "?";

                DataBind();
			} 
			else 
			{
                int nodeId = int.Parse(helper.Request("nodeId"));
                //ensure they have access to create under this node!!
                if (helper.Request("app") == Constants.Applications.Media || CheckCreatePermissions(nodeId))
                {
                    //pane_chooseName.Text = ui.Text("create", "updateData", this.getUser());
                    var c = new CMSNode(nodeId);
                    path.Value = c.Path;
                    pane_chooseNode.Visible = false;
                    panel_buttons.Visible = false;
                    pane_chooseName.Visible = true;
                    var createDef = new XmlDocument();
                    var defReader = new XmlTextReader(Server.MapPath(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/config/create/UI.xml"));
                    createDef.Load(defReader);
                    defReader.Close();

                    // Find definition for current nodeType
                    XmlNode def = createDef.SelectSingleNode("//nodeType [@alias = '" + Request.QueryString["app"] + "']");
                    phCreate.Controls.Add(new UserControl().LoadControl(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + def.SelectSingleNode("./usercontrol").FirstChild.Value));
                }
                else
                {                    
                    PageNameHolder.type = umbraco.uicontrols.Feedback.feedbacktype.error;
                    PageNameHolder.Text = ui.GetText("rights") + " " + ui.GetText("error");
                    JTree.DataBind();
                }
			}
                        
		}

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference( IOHelper.ResolveUrl( SystemDirectories.WebServices) +"/cmsnode.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference( IOHelper.ResolveUrl( SystemDirectories.WebServices) +"/legacyAjaxCalls.asmx"));
        }

        private bool CheckCreatePermissions(int nodeId)
        {
            return UmbracoEnsuredPage.CurrentUser.GetPermissions(new CMSNode(nodeId).Path)
                .Contains(ActionNew.Instance.Letter.ToString());
        }

	}
}
