using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Text;
using System.IO;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ModuleInjectionMacroRenderer : UmbracoDefault
    {


        protected override void Render(HtmlTextWriter output)
        {
            if (!string.IsNullOrEmpty(Request["tag"]))
            {
                presentation.templateControls.Macro m = new presentation.templateControls.Macro();

                Hashtable DataValues = helper.ReturnAttributes(Request["tag"]);

                m.Alias = DataValues["alias"].ToString();
                m.MacroAttributes = DataValues;

                StringBuilder sb = new StringBuilder();
                StringWriter tw = new StringWriter(sb);
                HtmlTextWriter hw = new HtmlTextWriter(tw);

                m.RenderControl(hw);

                Response.Output.Write(sb.ToString());
            }
        }
    }
}