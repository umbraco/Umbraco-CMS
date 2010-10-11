using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections;

namespace umbraco.presentation.templateControls {

    [DefaultProperty("Alias")]
    [ToolboxData("<{0}:Macro runat=server></{0}:Macro>")]
    public class Macro : WebControl {

        private Hashtable m_Attributes = new Hashtable();

        public Hashtable MacroAttributes {
            get {
//                Hashtable attributes = (Hashtable)ViewState["Attributes"];
                return m_Attributes;
            }
            set
            {
                m_Attributes = value;
            }
        }


        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Alias {
            get {
                String s = (String)ViewState["Alias"];
                return ((s == null) ? String.Empty : s);
            }

            set {
                ViewState["Alias"] = value;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Make sure child controls are in place to receive postback data.
            EnsureChildControls();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            // collect all attributes set on the control
            if (MacroAttributes == null || MacroAttributes.Count == 0) {
                ICollection keys = Attributes.Keys;
                foreach(string key in keys) {
                    MacroAttributes.Add(key.ToLower(), Attributes[key]);
                }
            }
            if (!MacroAttributes.ContainsKey("macroalias") && !MacroAttributes.ContainsKey("macroAlias"))
                MacroAttributes.Add("macroalias", Alias);
            macro tempMacro = null;
            tempMacro = macro.ReturnFromAlias(Alias);

            // set pageId to int.MinValue if no pageID was found,
            // e.g. if the macro was rendered on a custom (non-Umbraco) page
            int pageId = Context.Items["pageID"] == null ? int.MinValue : int.Parse(Context.Items["pageID"].ToString());

            if (tempMacro != null) {

                try {
                    Control c = tempMacro.renderMacro(MacroAttributes, (Hashtable)Context.Items["pageElements"], pageId);
                    if (c != null)
                        Controls.Add(c);
                    else
                        System.Web.HttpContext.Current.Trace.Warn("Template", "Result of macro " + tempMacro.Name + " is null");

                } catch (Exception ee) {
                    System.Web.HttpContext.Current.Trace.Warn("Template", "Error adding macro " + tempMacro.Name, ee);
                }
            }

        }

        /// <summary>
        /// Renders the control to the specified HTML writer.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> object that receives the control content.</param>
        protected override void Render(HtmlTextWriter writer) {
            EnsureChildControls();

            bool isDebug = GlobalSettings.DebugMode && (helper.Request("umbdebugshowtrace") != "" || helper.Request("umbdebug") != "");
            if (isDebug)
            {
                writer.Write("<div title=\"Macro Tag: '{0}'\" style=\"border: 1px solid #009;\">", Alias);
            }
            base.RenderChildren(writer);
            if (isDebug)
            {
                writer.Write("</div>");
            }
        }


    }
}
