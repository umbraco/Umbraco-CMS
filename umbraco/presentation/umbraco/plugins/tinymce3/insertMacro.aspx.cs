using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using umbraco.BasePages;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.DataLayer;
using umbraco.IO;

namespace umbraco.presentation.tinymce3
{
    /// <summary>
    /// Summary description for insertMacro.
    /// </summary>
    public partial class insertMacro : UmbracoEnsuredPage
    {
        protected Button Button1;
        private ArrayList _dataFields = new ArrayList();
        public Macro m;
        private string _scriptOnLoad = "";

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!String.IsNullOrEmpty(_scriptOnLoad))
            {
                jQueryReady.Text = _scriptOnLoad;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
			ClientLoader.DataBind();

            _scriptOnLoad = "";

            string reqMacroID = UmbracoContext.Current.Request["umb_macroID"];
            string reqMacroAlias = UmbracoContext.Current.Request["umb_macroAlias"];
            bool ignoreForm = string.IsNullOrEmpty(UmbracoContext.Current.Request["class"]);

            pane_insert.Text = ui.Text("insertMacro");
            Page.Title = ui.Text("insertMacro");

            if (!String.IsNullOrEmpty(reqMacroID) || !String.IsNullOrEmpty(reqMacroAlias))
            {

                pane_edit.Visible = true;
                pane_insert.Visible = false;
                edit_buttons.Visible = true;
                insert_buttons.Visible = false;

                // Put user code to initialize the page here
                if (!string.IsNullOrEmpty(reqMacroID))
                {
                    m = new Macro(int.Parse(reqMacroID));
                }
                else
                {
                    m = new Macro(reqMacroAlias);
                }

                pane_edit.Text = ui.Text("edit") + " " + m.Name;
                Page.Title = ui.Text("edit") + " " + m.Name;

                String macroAssembly = "";
                String macroType = "";

                if (m.Properties.Length == 0)
                {

                    if (ignoreForm)
                    {
                        renderMacro_Click(null, EventArgs.Empty);
                    }
                    else
                    {
                        Literal fb = new Literal();
                        fb.Text = "<p>" + ui.Text("macroDoesNotHaveProperties") + "</p><p><a href='#' onClick='tinyMCEPopup.close();'>" + ui.Text("closeThisWindow") + "</a>";
                        macroProperties.Controls.Add(fb);
                        edit_buttons.Visible = false;
                    }

                }
                else
                {
                    foreach (MacroProperty mp in m.Properties)
                    {

                        macroAssembly = mp.Type.Assembly;
                        macroType = mp.Type.Type;
                        try
                        {
                            Assembly assembly = Assembly.LoadFrom( IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                            Type type = assembly.GetType(macroAssembly + "." + macroType);
                            IMacroGuiRendering typeInstance = Activator.CreateInstance(type) as IMacroGuiRendering;
                            if (typeInstance != null)
                            {
                                Control control = Activator.CreateInstance(type) as Control;
                                control.ID = mp.Alias;

                                if (!IsPostBack)
                                {
                                    string propertyValue = Request["umb_" + mp.Alias];
                                    if (propertyValue != null)
                                    {
                                        // replace linebreaks and quotes
                                        propertyValue =
                                            propertyValue.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\\"", "\"");

                                        // check encoding
                                        propertyValue = HttpUtility.UrlDecode(propertyValue);

                                        if (propertyValue != "")
                                        {
                                            type.GetProperty("Value").SetValue(control,
                                                                               Convert.ChangeType(
                                                                                   propertyValue,
                                                                                   type.GetProperty("Value").PropertyType),
                                                                               null);
                                        }
                                    }
                                }


                                uicontrols.PropertyPanel pp = new global::umbraco.uicontrols.PropertyPanel();
                                pp.Text = mp.Name;
                                pp.Controls.Add(control);
                                _scriptOnLoad += "\t\tregisterAlias('" + control.ID + "');\n";
//                                pp.Controls.Add(new LiteralControl("<script type=\"text/javascript\"></script>\n"));
                                macroProperties.Controls.Add(pp);

                                _dataFields.Add(control);

                                //macroProperties.Controls.Add(new LiteralControl("</td></tr>"));
                            }
                            else
                            {
                                Trace.Warn("umbEditContent",
                                           "Type doesn't exist or is not umbraco.interfaces.DataFieldI ('" + macroAssembly +
                                           "." + macroType + "')");
                            }
                        }
                        catch (Exception fieldException)
                        {
                            Trace.Warn("umbEditContent", "Error creating type '" + macroAssembly + "." + macroType + "'",
                                       fieldException);
                        }
                    }
                }
            }
            else
            {
                IRecordsReader macroRenderings;
                if (UmbracoContext.Current.Request["editor"] != "")
                    macroRenderings = SqlHelper.ExecuteReader("select macroAlias, macroName from cmsMacro where macroUseInEditor = 1 order by macroName");
                else
                    macroRenderings = SqlHelper.ExecuteReader("select macroAlias, macroName from cmsMacro order by macroName");

                umb_macroAlias.DataSource = macroRenderings;
                umb_macroAlias.DataValueField = "macroAlias";
                umb_macroAlias.DataTextField = "macroName";
                umb_macroAlias.DataBind();
                macroRenderings.Close();
            }
        }


        protected void renderMacro_Click(object sender, EventArgs e)
        {
            int pageID = int.Parse(UmbracoContext.Current.Request["umbPageId"]);
            string macroAttributes = "macroAlias=\"" + m.Alias + "\"";

            Guid pageVersion = new Guid(UmbracoContext.Current.Request["umbVersionId"]);

            Hashtable attributes = new Hashtable();
            attributes.Add("macroAlias", m.Alias);

            macro mRender = new macro(m.Id);
            foreach (Control c in _dataFields)
            {
                try
                {
                    IMacroGuiRendering ic = (IMacroGuiRendering)c;
                    attributes.Add(c.ID.ToLower(), ic.Value);
                    macroAttributes += " " + c.ID + "=\"" +
                                       ic.Value.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
                }
                catch
                {
                }
            }

            // document this, for gods sake!
            HttpContext.Current.Items["macrosAdded"] = 0;
            HttpContext.Current.Items["pageID"] = pageID.ToString();


            page p = new page(pageID, pageVersion);

            string div = macro.renderMacroStartTag(attributes, pageID, pageVersion).Replace("\\", "\\\\").Replace("'", "\\'");

            string macroContent =
                macro.MacroContentByHttp(pageID, pageVersion, attributes).Replace("\\", "\\\\").Replace("'", "\\'").
                    Replace("/", "\\/").Replace("\n", "\\n");

            if (macroContent.Length > 0 && macroContent.ToLower().IndexOf("<script") > -1)
                macroContent =
                    "<b>Macro rendering contains script code</b><br/>This macro won\\'t be rendered in the editor because it contains script code. It will render correct during runtime.";
            div += macroContent;
            div += macro.renderMacroEndTag();

            _scriptOnLoad += "\t\tumbracoEditMacroDo('" + macroAttributes.Replace("'", "\\'") +
                                               "', '" + m.Name.Replace("'", "\\'") + "', '" + div + "');\n";
/*
            ClientScript.RegisterStartupScript(GetType(), "postbackScript",
                                               "<script>\n umbracoEditMacroDo('" + macroAttributes.Replace("'", "\\'") +
                                               "', '" + m.Name.Replace("'", "\\'") + "', '" + div + "');\n</script>");
            ClientScript.RegisterStartupScript(GetType(), "postbackScriptWindowClose",
                                               "<script>\n //setTimeout('window.close()',300);\n</script>");
*/            //theForm.Visible = false;
        }
    }
}