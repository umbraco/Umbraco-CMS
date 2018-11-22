using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.ComponentModel;
using umbraco.scripting;
using System.Collections;

namespace umbraco.presentation.templateControls
{
    [DefaultProperty("Language")]
    [ToolboxData("<{0}:Script runat=server></{0}:Script>")]
    public class Script : Literal
    {

        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("python")]
        [Localizable(true)]
        public string Language
        {
            get
            {
                String s = (String)ViewState["Language"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Language"] = value;
            }
        }


        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer)
        {
            EnsureChildControls();

            bool isDebug = GlobalSettings.DebugMode && (helper.Request("umbdebugshowtrace") != "" || helper.Request("umbdebug") != "");
            if (isDebug)
            {
                writer.Write("<div title=\"Script Tag\" style=\"border: 1px solid #009;\">");
            }

            Hashtable attr = new Hashtable((Hashtable)Context.Items["pageElements"]);
            attr.Add("currentPage", NodeFactory.Node.GetCurrent());

            // TODO: Hook the new MacroEngine
            string result = ""; //  MacroScript.Execute(this.Text, this.Language, attr);

            writer.Write(result);

            if (isDebug)
            {
                writer.Write("</div>");
            }
        }
    
    }
}
