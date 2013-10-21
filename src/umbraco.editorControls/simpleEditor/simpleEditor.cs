using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;
using Umbraco.Core.IO;

namespace umbraco.editorControls.simpleEditor
{
	/// <summary>
	/// Summary description for simpleEditor.
	/// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class SimpleEditor : System.Web.UI.WebControls.TextBox, interfaces.IDataEditor
	{
		private interfaces.IData _data;
		


		public SimpleEditor(interfaces.IData Data) 
		{
			_data = Data;

		}

		public virtual bool TreatAsRichTextEditor 
		{
			get {return false;}
		}

		public bool ShowLabel 
		{
			get {return true;}
		}

		public Control Editor {get{return this;}}

		public void Save() 
		{
			_data.Value = this.Text;
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			base.TextMode = System.Web.UI.WebControls.TextBoxMode.MultiLine;
			base.Rows = 8;
			if (this.CssClass == "")
				this.CssClass = "umbEditorTextField";
			if (_data != null && _data.Value != null)
				Text = _data.Value.ToString();


		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
            string script = "function insertLink(element) {"+
                "var theLink = prompt('Enter URL for link here:','http://');" +
                "insertTag(element, 'a', ' href=\"' + theLink + '\"')}";
            script += @"
            function insertTag(element, tag, param) {
            start = '<' + tag + param + '>';
            eind = '</' + tag + '>';
             /* element = document.getElementById(element); */
              if (document.selection) {
                element.focus();
                sel = document.selection.createRange();
                sel.text = start + sel.text + eind;
              } else if (element.selectionStart || element.selectionStart == '0') {
                element.focus();
                var startPos = element.selectionStart;
                var endPos = element.selectionEnd;
                element.value = element.value.substring(0, startPos) + start + element.value.substring(startPos, endPos) + eind + element.value.substring(endPos, element.value.length);
              } else {
                element.value += start + eind;
              }
            }
            ";

            Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "simpleEditorJs",
                script, true);
           
		}

		protected override void Render(HtmlTextWriter output)
		{
			output.WriteLine("<table border=\"0\" cellspacing=\"0\" cellpadding=\"0\"><tr><td>");
		
			output.WriteLine(	"<p align=\"right\" style=\"margin: 0px; padding:0px\"><a href=\"javascript:insertLink(document.getElementById('" + this.ClientID + "'))\">" +
				"<img src=\"" + IOHelper.ResolveUrl(SystemDirectories.UmbracoClient) + "/simpleEditor/images/link.gif\" border=\"0\" align=\"right\" />" +
				"</a>" +
                "<a href=\"javascript:insertTag(document.getElementById('" + this.ClientID + "'), 'em', '')\">" +
                "<img src=\"" + IOHelper.ResolveUrl(SystemDirectories.UmbracoClient) + "/simpleEditor/images/italic.gif\" border=\"0\" style=\"margin-left: 3px; margin-right: 3px;\" align=\"right\" />" +
				"</a>" +
				" <a href=\"javascript:insertTag(document.getElementById('" + this.ClientID + "'), 'strong', '')\">" +
                "<img src=\"" + IOHelper.ResolveUrl(SystemDirectories.UmbracoClient) + "/simpleEditor/images/bold.gif\" border=\"0\" align=\"right\" />" +
				"</a>" +
				"<br/></p>");
			base.Render(output);
			output.WriteLine("</td></tr></table>");
		}
	}
}