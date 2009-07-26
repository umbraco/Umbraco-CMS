using System;
using System.Web.UI;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.presentation.Trees;
namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for mediaChooser.
    /// </summary>
    [ClientDependency(1, ClientDependencyType.Css, "js/submodal/submodal.css", true)]
    [ClientDependency(1, ClientDependencyType.Javascript, "js/submodal/common.js", true)]
    [ClientDependency(2, ClientDependencyType.Javascript, "js/submodal/submodal.js", true, "initPopUp")]
    [ClientDependency(3, ClientDependencyType.Javascript, "webservices/legacyAjaxCalls.asmx/js", true)]
    [ValidationProperty("Value")]
    public class mediaChooser : System.Web.UI.WebControls.HiddenField, interfaces.IDataEditor
    {
        interfaces.IData _data;
        public mediaChooser(interfaces.IData Data)
        {
            _data = Data;
        }

        public System.Web.UI.Control Editor { get { return this; } }
        #region IDataField Members

        //private string _text;

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
            if (base.Value.Trim() != "")
                _data.Value = base.Value;
            else
                _data.Value = null;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (_data != null && _data.Value != null && !String.IsNullOrEmpty(_data.Value.ToString()))
            {
                base.Value = _data.Value.ToString();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
			umbraco.presentation.ClientDependency.Controls.ClientDependencyLoader.RegisterDependency("js/submodal/common.js", "UmbracoRoot", umbraco.presentation.ClientDependency.ClientDependencyType.Javascript);
			umbraco.presentation.ClientDependency.Controls.ClientDependencyLoader.RegisterDependency("js/submodal/subModal.js", "UmbracoRoot", umbraco.presentation.ClientDependency.ClientDependencyType.Javascript);
			umbraco.presentation.ClientDependency.Controls.ClientDependencyLoader.RegisterDependency("js/submodal/subModal.css", "UmbracoRoot", umbraco.presentation.ClientDependency.ClientDependencyType.Css);			

            // We need to make sure we have a reference to the legacy ajax calls in the scriptmanager
            presentation.webservices.ajaxHelpers.EnsureLegacyCalls(base.Page);

        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {

            string tempTitle = "";
            string deleteLink = " &nbsp; <a href=\"javascript:" + this.ClientID + "_clear();\" style=\"color: red\">" + ui.Text("delete") + "</a> &nbsp; ";
            try
            {
                if (base.Value != "")
                {
                    tempTitle = new cms.businesslogic.CMSNode(int.Parse(base.Value)).Text;
                }
            }
            catch { }

            string strScript = "function " + this.ClientID + "_chooseId() {" +
                "\nshowPopWin('" + TreeService.GetPickerUrl(true,"media","media") + "', 300, 400, " + ClientID + "_saveId)" +
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
            if (base.Value == "")
                deleteLink = "";
            writer.WriteLine("<span id=\"" + this.ClientID + "_title\"><b>" + tempTitle + "</b>" + deleteLink + "</span><a href=\"javascript:" + this.ClientID + "_chooseId()\">" + ui.Text("choose") + "...</a> &nbsp; ");// &nbsp; <input type=\"hidden\" id=\"" + this.ClientID + "\" name=\"" + this.ClientID + "\" value=\"" + this.Text + "\">");
            base.Render(writer);
        }
        #endregion
    }
}
