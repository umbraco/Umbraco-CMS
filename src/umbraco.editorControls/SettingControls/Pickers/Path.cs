using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.datatype;
using System.Web.UI.WebControls;


namespace umbraco.editorControls.SettingControls.Pickers
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class Path : DataEditorSettingType
    {
        private PathPicker pp = new PathPicker();

        private string _val = string.Empty;
        public override string Value
        {
            get
            {
                return pp.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    _val = value;
            }
        }

        public override System.Web.UI.Control RenderControl(DataEditorSetting sender)
        {

            
            pp.ID = sender.GetName().Replace(" ", "_");

            if (string.IsNullOrEmpty(_val) && !string.IsNullOrEmpty(DefaultValue))
                pp.Value = DefaultValue;
            else
                pp.Value = _val;

            return pp;
        }
    }

    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class PathPicker : WebControl
    {
        private TextBox tb;

        public PathPicker()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {

            tb = new TextBox();
            tb.CssClass = "guiInputText guiInputStandardSize";
            tb.ID = this.ID + "input";
            this.Controls.Add(tb);

        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            tb.RenderControl(writer);

            writer.WriteLine(string.Format(" <a onclick=\"{0}\"href=\"javascript:void(0);\">Select</a>",
                string.Format("javascript:UmbClientMgr.openModalWindow('developer/packages/directoryBrowser.aspx?target={0}', 'Choose a file or a folder', true, 400, 500, 0, 0); return false;", tb.ClientID)));

        }

        private string _val = string.Empty;
        public string Value
        {
            get
            {
                return tb.Text;

            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _val = value;
                    tb.Text = _val;
                }
            }
        }
    }

}