using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using umbraco.DataLayer;

namespace umbraco.presentation.tinymce3
{
    /// <summary>
    /// Summary description for insertMacro.
    /// </summary>
    public partial class insertMacro : UmbracoEnsuredPage
    {
        protected Button Button1;
        private readonly ArrayList _dataFields = new ArrayList();
        public Macro m;
        private string _scriptOnLoad = "";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //this could be used for media or content so we need to at least validate that the user has access to one or the other
            if (!ValidateUserApp(DefaultApps.content.ToString()) && !ValidateUserApp(DefaultApps.media.ToString()))
                throw new UserAuthorizationException("The current user doesn't have access to the section/app");
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!string.IsNullOrEmpty(_scriptOnLoad))
            {
                jQueryReady.Text = _scriptOnLoad;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ClientLoader.DataBind();

            _scriptOnLoad = "";

            var reqMacroId = Request["umb_macroID"];
            var reqMacroAlias = Request["umb_macroAlias"];
            var ignoreForm = string.IsNullOrEmpty(Request["class"]);

            pane_insert.Text = ui.Text("insertMacro");
            Page.Title = ui.Text("insertMacro");

            if (!string.IsNullOrEmpty(reqMacroId) || !string.IsNullOrEmpty(reqMacroAlias))
            {

                pane_edit.Visible = true;
                pane_insert.Visible = false;
                edit_buttons.Visible = true;
                insert_buttons.Visible = false;

                // Put user code to initialize the page here
                if (!string.IsNullOrEmpty(reqMacroId))
                {
                    m = new Macro(int.Parse(reqMacroId));
                }
                else
                {
                    m = new Macro(reqMacroAlias);
                }

                pane_edit.Text = ui.Text("edit") + " " + m.Name;
                Page.Title = ui.Text("edit") + " " + m.Name;

                if (m.Properties.Length == 0)
                {

                    if (ignoreForm)
                    {
                        renderMacro_Click(null, EventArgs.Empty);
                    }
                    else
                    {
                        var fb = new Literal();
                        fb.Text = "<p>" + ui.Text("macroDoesNotHaveProperties") + "</p><p><a href='#' onClick='tinyMCEPopup.close();'>" + ui.Text("closeThisWindow") + "</a>";
                        macroProperties.Controls.Add(fb);
                        edit_buttons.Visible = false;
                    }

                }
                else
                {
                    foreach (var mp in m.Properties)
                    {

                        var macroAssembly = mp.Type.Assembly;
                        var macroType = mp.Type.Type;
                        try
                        {
                            var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                            var type = assembly.GetType(macroAssembly + "." + macroType);
                            var typeInstance = Activator.CreateInstance(type) as IMacroGuiRendering;
                            if (typeInstance != null)
                            {
                                var control = Activator.CreateInstance(type) as Control;
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


                                var pp = new uicontrols.PropertyPanel();
                                pp.Text = mp.Name;
                                pp.Controls.Add(control);
                                _scriptOnLoad += "\t\tregisterAlias('" + control.ID + "');\n";
                                macroProperties.Controls.Add(pp);

                                _dataFields.Add(control);

                                
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
                if (Request["editor"] != "")
                {
                    const string query = "select macroAlias, macroName from cmsMacro where macroUseInEditor = 1 order by macroName";
                    using (var sqlHelper = BusinessLogic.Application.SqlHelper)
                    using (var renderings = sqlHelper.ExecuteReader(query))
                        macroRenderings = renderings;
                }
                else
                {
                    const string query = "select macroAlias, macroName from cmsMacro order by macroName";
                    using (var sqlHelper = BusinessLogic.Application.SqlHelper)
                    using (var renderings = sqlHelper.ExecuteReader(query))
                        macroRenderings = renderings;
                }
                
                umb_macroAlias.DataSource = macroRenderings;
                umb_macroAlias.DataValueField = "macroAlias";
                umb_macroAlias.DataTextField = "macroName";
                umb_macroAlias.DataBind();
                macroRenderings.Close();
            }
        }


        protected void renderMacro_Click(object sender, EventArgs e)
        {
            var pageId = int.Parse(Request["umbPageId"]);

            var macroAttributes = string.Format("macroAlias=\"{0}\"", m.Alias);

            var pageVersion = new Guid(Request["umbVersionId"]);

            var attributes = new Hashtable { { "macroAlias", m.Alias } };

            foreach (Control c in _dataFields)
            {
                try
                {
                    var ic = (IMacroGuiRendering)c;
                    attributes.Add(c.ID.ToLower(), ic.Value);
                    macroAttributes += string.Format(" {0}=\"{1}\"", c.ID, ic.Value.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"));
                }
                catch
                {
                }
            }

            HttpContext.Current.Items["macrosAdded"] = 0;
            HttpContext.Current.Items["pageID"] = pageId.ToString();

            var div = macro.renderMacroStartTag(attributes, pageId, pageVersion).Replace("\\", "\\\\").Replace("'", "\\'");

            var macroContent = macro.MacroContentByHttp(pageId, pageVersion, attributes).Replace("\\", "\\\\").Replace("'", "\\'").Replace("/", "\\/").Replace("\n", "\\n");

            if (macroContent.Length > 0 && macroContent.ToLower().IndexOf("<script") > -1)
                macroContent = "<b>Macro rendering contains script code</b><br/>This macro won\\'t be rendered in the editor because it contains script code. It will render correct during runtime.";

            div += macroContent;
            div += macro.renderMacroEndTag();

            _scriptOnLoad += string.Format("\t\tumbracoEditMacroDo('{0}', '{1}', '{2}');\n", macroAttributes.Replace("'", "\\'"), m.Name.Replace("'", "\\'"), div);
        }
    }
}