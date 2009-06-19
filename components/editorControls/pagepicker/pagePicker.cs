using System;
using System.Web.UI;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.presentation.Trees;
namespace umbraco.editorControls
{
	/// <summary>
	/// Summary description for pagePicker.
	/// </summary>
    [ClientDependency(1, ClientDependencyType.Css, "js/submodal/submodal.css", true)]
    [ClientDependency(1, ClientDependencyType.Javascript, "js/submodal/common.js", true)]
    [ClientDependency(2, ClientDependencyType.Javascript, "js/submodal/submodal.js", true, "initPopUp")]
    [ClientDependency(3, ClientDependencyType.Javascript, "webservices/legacyAjaxCalls.asmx/js", true)]
    [ValidationProperty("Value")]
	public class pagePicker : System.Web.UI.WebControls.HiddenField, interfaces.IDataEditor
	{
		interfaces.IData _data;
		public pagePicker(interfaces.IData Data)
		{
			_data = Data;
		}
		#region IDataField Members

		//private string _text;


		public System.Web.UI.Control Editor { get { return this; } }

		public virtual bool TreatAsRichTextEditor
		{
			get { return false; }
		}
		public bool ShowLabel
		{
			get
			{
				return true;
			}
		}

		public void Save()
		{
			//_text = helper.Request(this.ClientID);
			if (base.Value.Trim() != "")
				_data.Value = base.Value.Trim();
			else
				_data.Value = null;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			base.Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "subModal", "<script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/submodal/common.js\"></script><script type=\"text/javascript\" src=\"" + GlobalSettings.Path + "/js/submodal/subModal.js\"></script><link href=\"" + GlobalSettings.Path + "/js/submodal/subModal.css\" type=\"text/css\" rel=\"stylesheet\"></link>");

            // We need to make sure we have a reference to the legacy ajax calls in the scriptmanager
            presentation.webservices.ajaxHelpers.EnsureLegacyCalls(base.Page);
			
            if (_data != null && _data.Value != null)
				base.Value = _data.Value.ToString();
		}


		protected override void Render(System.Web.UI.HtmlTextWriter writer)
		{

			string tempTitle = "";
			string deleteLink = " &nbsp; <a href=\"javascript:" + this.ClientID + "_clear();\" style=\"color: red\">" + ui.Text("delete") + "</a> &nbsp; ";
			try
			{
				if (base.Value.Trim() != "")
				{
					tempTitle = new cms.businesslogic.CMSNode(int.Parse(base.Value.Trim())).Text;
				}
			}
			catch { }

			string strScript = "function " + this.ClientID + "_chooseId() {" +
				"\nshowPopWin('" + TreeService.GetPickerUrl(true,"content","content") + "', 300, 400, " + ClientID + "_saveId);" +
				//"\nvar treePicker = window.showModalDialog(, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')			" +
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
				"\n}";

			try
			{
				if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
					ScriptManager.RegisterClientScriptBlock(this, this.GetType(), this.ClientID + "_chooseId", strScript, true);
				else
					Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_chooseId", strScript, true);
			}
			catch
			{
				Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "_chooseId", strScript, true);
			}
			// Clear remove link if text if empty
			if (base.Value.Trim() == "")
				deleteLink = "";
			writer.WriteLine("<span id=\"" + this.ClientID + "_title\"><b>" + tempTitle + "</b>" + deleteLink + "</span><a href=\"javascript:" + this.ClientID + "_chooseId()\">" + ui.Text("choose") + "...</a> &nbsp; ");//<input type=\"hidden\" id=\"" + this.ClientID + "\" name=\"" + this.ClientID + "\" value=\"" + this.Text + "\">");
			base.Render(writer);
		}
		#endregion
	}
}
