using System;
using System.Data;
using Microsoft.ApplicationBlocks.Data;
using System.Data.SqlClient;
using System.Web.UI;
using ClientDependency.Core;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Summary description for content.
	/// </summary>
	[ClientDependency(103, ClientDependencyType.Javascript, "js/submodal/common.js", "UmbracoRoot")]
	[ClientDependency(104, ClientDependencyType.Javascript, "js/submodal/subModal.js", "UmbracoRoot")]
	[ClientDependency(ClientDependencyType.Css, "js/submodal/subModal.css", "UmbracoRoot")]
    public class content : System.Web.UI.WebControls.WebControl, interfaces.IMacroGuiRendering
	{
		private string m_value = "";

		public bool ShowCaption 
		{
			get {return true;}
		}

		public string Value
		{
            get {
                if (Page.IsPostBack && !String.IsNullOrEmpty(System.Web.HttpContext.Current.Request[this.ClientID])) {
                    m_value = System.Web.HttpContext.Current.Request[this.ClientID];
                }
                return m_value;
            }
            set { m_value = value; }
		}

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);
            
            // We need to make sure we have a reference to the legacy ajax calls in the scriptmanager
            ScriptManager sm = ScriptManager.GetCurrent(Page);
            ServiceReference legacyPath = new ServiceReference(GlobalSettings.Path + "/webservices/legacyAjaxCalls.asmx");
            if (!sm.Services.Contains(legacyPath))
                sm.Services.Add(legacyPath);
        }

		public content()
		{
			//
			// TODO: Add constructor logic here
			//
		}

        protected override void Render(System.Web.UI.HtmlTextWriter writer) {

            string tempTitle = "";
            string deleteLink = " &nbsp; <a href=\"javascript:" + this.ClientID + "_clear();\" style=\"color: red\">" + ui.Text("delete") + "</a> &nbsp; ";
            try {
                if (this.Value != "") {
                    tempTitle = new cms.businesslogic.CMSNode(int.Parse(this.Value)).Text;
                }
            } catch { }

            writer.WriteLine("<script language=\"javascript\">\nfunction " + this.ClientID + "_chooseId() {" +
                "\nshowPopWin('" + GlobalSettings.Path + "/dialogs/treePicker.aspx?useSubModal=true&app=content&treeType=content', 300, 400, " + ClientID + "_saveId)" +
                //				"\nvar treePicker = window.showModalDialog(, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')			" +
                "\n}" +
                "\nfunction " + ClientID + "_saveId(treePicker) {" +
                "\nsetTimeout('" + ClientID + "_saveIdDo(' + treePicker + ')', 200);" +
                "\n}" +
                "\nfunction " + ClientID + "_saveIdDo(treePicker) {" +
                "\nif (treePicker != undefined) {" +
                "\ndocument.getElementById(\"" + this.ClientID + "\").value = treePicker;" +
                "\nif (treePicker > 0) {" +
                    "\numbraco.presentation.webservices.legacyAjaxCalls.GetNodeName(treePicker, " + this.ClientID + "_updateContentTitle" + ");" +
                "\n}				" +
                "\n}" +
                "\n}			" +
                "\nfunction " + this.ClientID + "_updateContentTitle(retVal) {" +
                "\ndocument.getElementById(\"" + this.ClientID + "_title\").innerHTML = \"<strong>\" + retVal + \"</strong>" + deleteLink.Replace("\"", "\\\"") + "\";" +
                "\n}" +
                "\nfunction " + this.ClientID + "_clear() {" +
                "\ndocument.getElementById(\"" + this.ClientID + "_title\").innerHTML = \"\";" +
                "\ndocument.getElementById(\"" + this.ClientID + "\").value = \"\";" +
                "\n}" +
                "\n</script>");

            // Clear remove link if text if empty
            if (this.Value == "")
                deleteLink = "";
            writer.WriteLine("<span id=\"" + this.ClientID + "_title\"><b>" + tempTitle + "</b>" + deleteLink + "</span><a href=\"javascript:" + this.ClientID + "_chooseId()\">" + ui.Text("choose") + "...</a> &nbsp; <input type=\"hidden\" id=\"" + this.ClientID + "\" name=\"" + this.ClientID + "\" value=\"" + this.Value + "\">");
            base.Render(writer);
        }


        /*
		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{
			string label = "";
			if (this.Value != "") 
			{
				SqlDataReader pageName = SqlHelper.ExecuteReader(umbraco.GlobalSettings.DbDSN, 
					CommandType.Text, "select text as nodeName from umbracoNode where id = " + this.Value);
				if (pageName.Read())
					label = pageName.GetString(pageName.GetOrdinal("nodeName")) + "<br/>";
				pageName.Close();
			}
			writer.WriteLine("<b><span id=\"label" + this.ID + "\">" + label + "</span></b>");
			
			writer.WriteLine("<a href=\"javascript:saveTreepickerValue('content','" + this.ID + "');\">Choose item</a>");
			writer.WriteLine("<input type=\"hidden\" name=\"" + this.ID + "\" value=\"" + this.Value + "\"/>");
		}*/

	}
}
