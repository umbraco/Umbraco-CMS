using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.datatype;
using System.Web.UI.WebControls;

namespace umbraco.editorControls.SettingControls
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class TextField : DataEditorSettingType
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
            tb.TextMode = TextBoxMode.SingleLine;
            tb.CssClass = "guiInputText guiInputStandardSize";


            if (string.IsNullOrEmpty(tb.Text) && !string.IsNullOrEmpty(DefaultValue))
                tb.Text = DefaultValue;

            return tb;
        }
    }
}
