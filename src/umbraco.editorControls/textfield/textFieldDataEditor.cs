using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.Collections;

namespace umbraco.editorControls.textfield
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class TextFieldEditor : TextBox, interfaces.IDataEditor
    {
        private interfaces.IData _data;


        public TextFieldEditor(interfaces.IData Data)
        {
            _data = Data;
        }

        public virtual bool TreatAsRichTextEditor
        {
            get { return false; }
        }

        public bool ShowLabel
        {
            get { return true; }
        }

        public Control Editor { get { return this; } }

        public void Save()
        {
            //GE 2012-01-18 
            //allow the textboxmultiple datatype to contain CDATA tags
            _data.Value = this.Text.Replace("<![CDATA[", "<!--CDATAOPENTAG-->").Replace("]]>", "<!--CDATACLOSETAG-->");
        }

        protected override void OnInit(EventArgs e)
        {
            if (this.CssClass == "")
                this.CssClass = "umbEditorTextField";

            //GE 2012-01-18 
            //allow the textboxmultiple datatype to contain CDATA tags
            if (_data != null && _data.Value != null)
                Text = _data.Value.ToString().Replace("<!--CDATAOPENTAG-->", "<![CDATA[").Replace("<!--CDATACLOSETAG-->", "]]>");

            base.OnInit(e);
        }
    }
}