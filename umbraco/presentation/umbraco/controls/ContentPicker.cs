using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using umbraco.cms.presentation.Trees;
using umbraco.presentation.ClientDependency;

namespace umbraco.controls
{

	//"<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/webservices/ajax.js\"></script>");
	//"<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/xmlextras.js\"></script>"
	//"<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/xmlRequest.js\"></script>");
	//"<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/submodal/common.js\"></script>
	//"<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/submodal/subModal.js\"></script>
	//"<link href=\"" + GlobalSettings.Path + "/js/submodal/subModal.css\" type=\"text/css\" rel=\"stylesheet\"></link>");


	[ClientDependency(ClientDependencyType.Javascript, "js/xmlextras.js", "UmbracoRoot")]
	[ClientDependency(ClientDependencyType.Javascript, "js/xmlRequest.js", "UmbracoRoot")]
	[ClientDependency(ClientDependencyType.Javascript, "webservices/ajax.js", "UmbracoRoot")]
	[ClientDependency(ClientDependencyType.Javascript, "js/submodal/common.js", "UmbracoRoot")]
	[ClientDependency(ClientDependencyType.Javascript, "js/submodal/subModal.js", "UmbracoRoot")]
	[ClientDependency(ClientDependencyType.Css, "js/submodal/subModal.css", "UmbracoRoot")]
	public class ContentPicker : System.Web.UI.WebControls.WebControl
	{

		public System.Web.UI.Control Editor { get { return this; } }

		private string _text = "";
		public string Text
		{
			get
			{
				if (Page.IsPostBack && !String.IsNullOrEmpty(helper.Request(this.ClientID)))
				{
					_text = helper.Request(this.ClientID);
				}
				return _text;
			}
			set { _text = value; }
		}

		private string _appAlias = "content";
		public string AppAlias
		{
			get { return _appAlias; }
			set { _appAlias = value; }
		}


		private string _treeAlias = "content";
		public string TreeAlias
		{
			get { return _treeAlias; }
			set { _treeAlias = value; }
		}

		private bool _showDelete = true;
		public bool ShowDelete
		{
			get { return _showDelete; }
			set { _showDelete = value; }
		}

		private int m_modalWidth = 300;
		public int ModalWidth
		{
			get { return m_modalWidth; }
			set { m_modalWidth = value; }
		}

		private int m_modalHeight = 400;
		public int ModalHeight
		{
			get { return m_modalHeight; }
			set { m_modalHeight = value; }
		}


		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			//base.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ajax", "<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/webservices/ajax.js\"></script>");
			//base.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "ajax1", "<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/xmlextras.js\"></script><script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/xmlRequest.js\"></script>");
			//base.Page.ClientScript.RegisterClientScriptBlock(GetType(), "subModal", "<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/submodal/common.js\"></script><script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/submodal/subModal.js\"></script><link href=\"" + GlobalSettings.Path + "/js/submodal/subModal.css\" type=\"text/css\" rel=\"stylesheet\"></link>");

			// We need to make sure we have a reference to the legacy ajax calls in the scriptmanager
			presentation.webservices.ajaxHelpers.EnsureLegacyCalls(base.Page);
		}


		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{

			string tempTitle = "";
			string deleteLink = " &nbsp; <a href=\"javascript:" + this.ClientID + "_clear();\" style=\"color: red\">" + ui.Text("delete") + "</a> &nbsp; ";
			try
			{
				if (this.Text != "" && this.Text != "-1")
				{
					tempTitle = new cms.businesslogic.CMSNode(int.Parse(this.Text)).Text;
				}
				else
				{
					tempTitle = (!string.IsNullOrEmpty(_treeAlias) ? ui.Text(_treeAlias) : ui.Text(_appAlias));

				}
			}
			catch { }

			writer.WriteLine("<script language=\"javascript\">\nfunction " + this.ClientID + "_chooseId() {" +
				"\nshowPopWin('" + TreeService.GetPickerUrl(true, _appAlias, _treeAlias) + "', " + m_modalWidth + ", " + m_modalHeight + ", " + ClientID + "_saveId)" +
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
				"\ndocument.getElementById(\"" + this.ClientID + "\").value = \"-1\";" +
				"\n}" +
				"\n</script>");

			// Clear remove link if text if empty
			if (this.Text == "" || !_showDelete)
				deleteLink = "";
			writer.WriteLine("<span id=\"" + this.ClientID + "_title\"><b>" + tempTitle + "</b>" + deleteLink + "</span> <a href=\"javascript:" + this.ClientID + "_chooseId()\">" + ui.Text("choose") + "...</a> &nbsp; <input type=\"hidden\" id=\"" + this.ClientID + "\" name=\"" + this.ClientID + "\" value=\"" + this.Text + "\">");
			base.Render(writer);
		}

	}
}
