using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;

namespace umbraco.presentation.umbraco.LiveEditing.Modules.SkinModule
{
    public partial class ModuleInjector : BasePages.UmbracoEnsuredPage
    {
        private cms.businesslogic.macro.Macro _m;
        public string _macroAlias = "";

        public ModuleInjector()
        {
            //for skinning, you need to be a developer
            CurrentApp = DefaultApps.developer.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            renderProperties();
        }

        protected void renderProperties()
        {
            if (!string.IsNullOrEmpty(Request["macroAlias"]))
            {


                _m = cms.businesslogic.macro.Macro.GetByAlias(Request["macroAlias"]);

                _macroAlias = _m.Alias;


                //If no properties, we will exit now...
                if (_m.Properties.Length == 0)
                {
                    var noProps = new Literal();
                    noProps.Text = "<script type='text/javascript'>updateMacro()</script>";
                    macroProperties.Controls.Add(noProps);
                }
                else
                {
                    //if we have properties, we'll render the controls for them...
                    foreach (cms.businesslogic.macro.MacroProperty mp in _m.Properties)
                    {
                        var macroAssembly = mp.Type.Assembly;
                        var macroType = mp.Type.Type;
                        try
                        {

                            var assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                            var type = assembly.GetType(macroAssembly + "." + macroType);
                            var typeInstance = Activator.CreateInstance(type) as interfaces.IMacroGuiRendering;
                            if (typeInstance != null)
                            {
                                var control = Activator.CreateInstance(type) as Control;
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
                                var pp = new uicontrols.PropertyPanel();
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
        }
    }
}