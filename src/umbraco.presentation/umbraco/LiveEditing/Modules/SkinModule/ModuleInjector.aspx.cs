using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using umbraco.IO;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ModuleInjector : BasePages.UmbracoEnsuredPage
    {
        private cms.businesslogic.macro.Macro m;
        public string _macroAlias = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            renderProperties();
        }

        protected void renderProperties()
        {
            if (!string.IsNullOrEmpty(Request["macroAlias"]))
            {


                m = cms.businesslogic.macro.Macro.GetByAlias(Request["macroAlias"]);

                String macroAssembly = "";
                String macroType = "";

                _macroAlias = m.Alias;


                //If no properties, we will exit now...
                if (m.Properties.Length == 0)
                {
                    Literal noProps = new Literal();
                    noProps.Text = "<script type='text/javascript'>updateMacro()</script>";
                    macroProperties.Controls.Add(noProps);
                }
                else
                {
                    //if we have properties, we'll render the controls for them...
                    foreach (cms.businesslogic.macro.MacroProperty mp in m.Properties)
                    {
                        macroAssembly = mp.Type.Assembly;
                        macroType = mp.Type.Type;
                        try
                        {

                            Assembly assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                            Type type = assembly.GetType(macroAssembly + "." + macroType);
                            interfaces.IMacroGuiRendering typeInstance = Activator.CreateInstance(type) as interfaces.IMacroGuiRendering;
                            if (typeInstance != null)
                            {
                                Control control = Activator.CreateInstance(type) as Control;
                                control.ID = mp.Alias;

                                if (!IsPostBack)
                                {
                                    if (Request["umb_" + mp.Alias] != null)
                                    {
                                        if (Request["umb_" + mp.Alias] != "")
                                        {
                                            type.GetProperty("Value").SetValue(control, Convert.ChangeType(Request["umb_" + mp.Alias], type.GetProperty("Value").PropertyType), null);
                                        }
                                    }
                                }

                                // register alias
                                uicontrols.PropertyPanel pp = new uicontrols.PropertyPanel();
                                pp.Text = mp.Name;
                                pp.Controls.Add(control);
                                macroProperties.Controls.Add(pp);

                                pp.Controls.Add(new LiteralControl("<script type=\"text/javascript\"> registerAlias('" + control.ClientID + "','" + mp.Alias + "'); </script>\n"));



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
            }
            else
            {
              
            }
        }
    }
}