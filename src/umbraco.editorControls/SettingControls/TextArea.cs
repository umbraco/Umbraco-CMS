using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class TextArea : DataEditorSettingType
    {
        private TextBox tb = new TextBox();

        public override string Value
        {
            get
            {
                return tb.Text;
            }
            set
            {
                tb.Text = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {
            tb.ID = sender.GetName();
            tb.TextMode = TextBoxMode.MultiLine;
            tb.CssClass = "guiInputText guiInputStandardSize";
            tb.Rows = 7;

            if (string.IsNullOrEmpty(tb.Text) && this.Prevalues.Count > 0)
                tb.Text = this.Prevalues[0];
            else if (string.IsNullOrEmpty(tb.Text) && !string.IsNullOrEmpty(DefaultValue))
                tb.Text = DefaultValue;


            return tb;
        }
    }
}