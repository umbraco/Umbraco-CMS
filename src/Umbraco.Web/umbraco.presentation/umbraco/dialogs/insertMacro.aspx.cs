using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Reflection;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using umbraco.businesslogic.Exceptions;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for insertMacro.
    /// </summary>
    public partial class insertMacro : BasePages.UmbracoEnsuredPage
    {
        protected Button Button1;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //this could be used for media or content so we need to at least validate that the user has access to one or the other
            if (!ValidateUserApp(DefaultApps.content.ToString()) && !ValidateUserApp(DefaultApps.media.ToString()))
                throw new UserAuthorizationException("The current user doesn't have access to the section/app");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            pane_edit.Text = ui.Text("general", "edit", this.getUser()) + " " + ui.Text("general", "macro", this.getUser());
            pane_insert.Text = ui.Text("general", "insert", this.getUser()) + " " + ui.Text("general", "macro", this.getUser());

            if (Request["macroID"] != null || Request["macroAlias"] != null)
            {
                // Put user code to initialize the page here
                cms.businesslogic.macro.Macro m;
                if (helper.Request("macroID") != "")
                    m = new cms.businesslogic.macro.Macro(int.Parse(helper.Request("macroID")));
                else
                    m = cms.businesslogic.macro.Macro.GetByAlias(helper.Request("macroAlias"));

			    foreach (var mp in m.Properties) {

                    var macroAssembly = mp.Type.Assembly;
                    var macroType = mp.Type.Type;
                    try
                    {

                        var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                        Type type = assembly.GetType(macroAssembly + "." + macroType);
                        var typeInstance = Activator.CreateInstance(type) as interfaces.IMacroGuiRendering;
                        if (typeInstance != null)
                        {
                            var control = Activator.CreateInstance(type) as Control;
                            control.ID = mp.Alias;
                            if (Request[mp.Alias] != null)
                            {
                                if (Request[mp.Alias] != "")
                                {
                                    type.GetProperty("Value").SetValue(control, Convert.ChangeType(Request[mp.Alias], type.GetProperty("Value").PropertyType), null);
                                }
                            }

                            // register alias
                            var pp = new uicontrols.PropertyPanel();
                            pp.Text = mp.Name;
                            pp.Controls.Add(control);

                            macroProperties.Controls.Add(pp);

                            /*
							macroProperties.Controls.Add(new LiteralControl("<script>\nregisterAlias('" + control.ID + "');\n</script>"));
							macroProperties.Controls.Add(new LiteralControl("<tr><td class=\"propertyHeader\">" + mp.Name + "</td><td class=\"propertyContent\">"));
							macroProperties.Controls.Add(control);
							macroProperties.Controls.Add(new LiteralControl("</td></tr>"));
                            */
                        }
                        else
                        {
                            Trace.Warn("umbEditContent", "Type doesn't exist or is not umbraco.interfaces.DataFieldI ('" + macroAssembly + "." + macroType + "')");
                        }

                    }
                    catch (Exception fieldException)
                    {
                        Trace.Warn("umbEditContent", "Error creating type '" + macroAssembly + "." + macroType + "'", fieldException);
                    }
                }
            }
            else
            {
                IRecordsReader macroRenderings;
				if (helper.Request("editor") != "")
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

                macroAlias.DataSource = macroRenderings;
                macroAlias.DataValueField = "macroAlias";
                macroAlias.DataTextField = "macroName";
                macroAlias.DataBind();
                macroRenderings.Close();
            }
        }
    }
}
