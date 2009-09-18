using System;
using System.Web.UI;

using umbraco.cms.presentation.Trees;
using ClientDependency.Core;
using umbraco.presentation;
using ClientDependency.Core.Controls;
namespace umbraco.editorControls
{
    /// <summary>
    /// Summary description for mediaChooser.
    /// </summary>
    [ClientDependency(100, ClientDependencyType.Css, "js/submodal/submodal.css", "UmbracoRoot")]
	[ClientDependency(101, ClientDependencyType.Javascript, "js/submodal/common.js", "UmbracoRoot")]
	[ClientDependency(102, ClientDependencyType.Javascript, "js/submodal/submodal.js", "UmbracoRoot", InvokeJavascriptMethodOnLoad = "initPopUp")]	
	[ValidationProperty("Value")]
    public class mediaChooser : System.Web.UI.WebControls.HiddenField, interfaces.IDataEditor
    {
        interfaces.IData _data;
        bool _showpreview;
        bool _showadvanced;

        public mediaChooser(interfaces.IData Data)
        {
            _data = Data;
        }

        public mediaChooser(interfaces.IData Data, bool ShowPreview, bool ShowAdvanced)
        {
            _data = Data;
            _showpreview = ShowPreview;
            _showadvanced = ShowAdvanced;
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

            // We need to make sure we have a reference to the legacy ajax calls in the scriptmanager
			if (!UmbracoContext.Current.LiveEditingContext.Enabled)
				presentation.webservices.ajaxHelpers.EnsureLegacyCalls(base.Page);
			else
				ClientDependencyLoader.Instance.RegisterDependency("webservices/legacyAjaxCalls.asmx/js", "UmbracoRoot", ClientDependencyType.Javascript);

            // And a reference to the media picker calls 
            if (!UmbracoContext.Current.LiveEditingContext.Enabled)
            {
                ScriptManager sm = ScriptManager.GetCurrent(base.Page);
                ServiceReference webservicePath = new ServiceReference(GlobalSettings.Path + "/webservices/MediaPickerService.asmx");

                if (!sm.Services.Contains(webservicePath))
                    sm.Services.Add(webservicePath);
            }
            else
            {
                ClientDependencyLoader.Instance.RegisterDependency("webservices/MediaPickerService.asmx/js", "UmbracoRoot", ClientDependencyType.Javascript);
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {

            string tempTitle = "";
            int mediaId = -1;
            string deleteLink = " &nbsp; <a href=\"javascript:" + this.ClientID + "_clear();\" style=\"color: red\">" + ui.Text("delete") + "</a> &nbsp; ";
            try
            {
                if (base.Value != "")
                {
                    mediaId = int.Parse(base.Value);
                    tempTitle = new cms.businesslogic.CMSNode(int.Parse(base.Value)).Text;
                }
            }
            catch { }

            string dialog = "\nshowPopWin('" + TreeService.GetPickerUrl(true, "media", "media") + "', 300, 400, " + ClientID + "_saveId)";
            if (_showadvanced)
                dialog = "\nshowPopWin('" + GlobalSettings.Path + "/dialogs/mediaPicker.aspx" + "', 500, 530, " + ClientID + "_saveId)";

            string preview = string.Empty;
            if (_showpreview)
                preview = "\numbraco.presentation.webservices.MediaPickerService.GetThumbNail(treePicker, " + this.ClientID + "_UpdateThumbNail);" +
                    "\numbraco.presentation.webservices.MediaPickerService.GetFile(treePicker, " + this.ClientID + "_UpdateLink);";


            string strScript = "function " + this.ClientID + "_chooseId() {" +
                //"\nshowPopWin('" + TreeService.GetPickerUrl(true, "media", "media") + "', 300, 400, " + ClientID + "_saveId)" +
                //"\nshowPopWin('" + GlobalSettings.Path + "/dialogs/mediaPicker.aspx" + "', 500, 530, " + ClientID + "_saveId)" +
                //				"\nvar treePicker = window.showModalDialog(, 'treePicker', 'dialogWidth=350px;dialogHeight=300px;scrollbars=no;center=yes;border=thin;help=no;status=no')			" +
                dialog +
                "\n}" +
                "\nfunction " + ClientID + "_saveId(treePicker) {" +
                "\nsetTimeout('" + ClientID + "_saveIdDo(' + treePicker + ')', 200);" +
                "\n}" +
                "\nfunction " + ClientID + "_saveIdDo(treePicker) {" +
                "\nif (treePicker != undefined) {" +
                    "\ndocument.getElementById(\"" + this.ClientID + "\").value = treePicker;" +
                    "\nif (treePicker > 0) {" +
                    "\numbraco.presentation.webservices.legacyAjaxCalls.GetNodeName(treePicker, " + this.ClientID + "_updateContentTitle" + ");" +
                    preview+                  
                    "\n}				" +
                "\n}" +
                "\n}			" +
                "\nfunction " + this.ClientID + "_updateContentTitle(retVal) {" +
                "\ndocument.getElementById(\"" + this.ClientID + "_title\").innerHTML = \"<strong>\" + retVal + \"</strong>" + deleteLink.Replace("\"", "\\\"") + "\";" +
                "\n}" +
                "\nfunction " + this.ClientID + "_clear() {" +
                "\ndocument.getElementById(\"" + this.ClientID + "_title\").innerHTML = \"\";" +
                "\ndocument.getElementById(\"" + this.ClientID + "\").value = \"\";" +
                "\ndocument.getElementById(\"" + this.ClientID + "_preview\").style.display = 'none';" +
                "\n}" +
                "\nfunction " + this.ClientID + "_UpdateThumbNail(retVal){" +      
                "\nif(retVal != \"\"){" +
                "\ndocument.getElementById(\"" + this.ClientID + "_thumbnail\").src = retVal;" +
                "\ndocument.getElementById(\"" + this.ClientID + "_preview\").style.display = 'block';}" +
                "\nelse{document.getElementById(\"" + this.ClientID + "_preview\").style.display = 'none';}" +
                "\n}"+
                "\nfunction " + this.ClientID + "_UpdateLink(retVal){" +
                "\ndocument.getElementById(\"" + this.ClientID + "_thumbnaillink\").href = retVal;" +
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



            //Thumbnail preview
            if (_showpreview)
            {
                string thumb = string.Empty;
                string link = string.Empty;
                string style = "display:none;";
                if (mediaId != -1)
                {
                    style = string.Empty;
                    thumb = string.Format(" src=\"{0}\" ", presentation.webservices.MediaPickerServiceHelpers.GetThumbNail(mediaId));
                    link = string.Format(" href=\"{0}\" ", presentation.webservices.MediaPickerServiceHelpers.GetFile(mediaId));
                }

                writer.WriteLine("<div id=\"" + this.ClientID + "_preview\" style=\"margin-top:5px;" + style + "\"><a " + link + "id=\"" + this.ClientID + "_thumbnaillink\" target=\"_blank\" ><img " + thumb + "id=\"" + this.ClientID + "_thumbnail\" /></a></div>");
            }
                
            
            
            
            base.Render(writer);
        }
        #endregion
    }
}
