using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Security;
using Umbraco.Core.IO;
using umbraco.DataLayer;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for insertMacro.
	/// </summary>
	public partial class insertMacro : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
	{
		protected Button Button1;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //this could be used for media or content so we need to at least validate that the user has access to one or the other
            if (Security.ValidateUserApp(Constants.Applications.Content) == false && Security.ValidateUserApp(Constants.Applications.Media) == false)
                throw new SecurityException("The current user doesn't have access to the section/app");
        }

		protected void Page_Load(object sender, EventArgs e)
		{
            pane_edit.Text = Services.TextService.Localize("general/edit") + " " + Services.TextService.Localize("general/macro");
            pane_insert.Text = Services.TextService.Localize("general/insert") + " " + Services.TextService.Localize("general/macro");

			if (Request["macroID"] != null || Request["macroAlias"] != null) 
			{
				// Put user code to initialize the page here
				cms.businesslogic.macro.Macro m;
				if (Request.GetItemAsString("macroID") != "")
					m = new cms.businesslogic.macro.Macro(int.Parse(Request.GetItemAsString("macroID")));
				else
					m = cms.businesslogic.macro.Macro.GetByAlias(Request.GetItemAsString("macroAlias"));

			    foreach (var mp in m.Properties) {
		
					var macroAssembly = mp.Type.Assembly;
					var macroType = mp.Type.Type;
					try 
					{

                        var assembly = Assembly.LoadFrom( IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

						Type type = assembly.GetType(macroAssembly+"."+macroType);
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
                List<dynamic> macroRenderings;
                if (Request.GetItemAsString("editor") != "")
					macroRenderings = DatabaseContext.Database.Fetch<dynamic>("select macroAlias, macroName from cmsMacro where macroUseInEditor = 1 order by macroName");
				else
					macroRenderings = DatabaseContext.Database.Fetch<dynamic>("select macroAlias, macroName from cmsMacro order by macroName");
				
				macroAlias.DataSource = macroRenderings;
				macroAlias.DataValueField = "macroAlias";
				macroAlias.DataTextField = "macroName";
				macroAlias.DataBind();

			}

		}
	}
}
