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
using umbraco.DataLayer;
using umbraco.IO;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for insertMacro.
	/// </summary>
	public partial class insertMacro : BasePages.UmbracoEnsuredPage
	{
		protected System.Web.UI.WebControls.Button Button1;
		protected void Page_Load(object sender, System.EventArgs e)
		{
            pane_edit.Text = ui.Text("general", "edit",this.getUser()) + " " + ui.Text("general", "macro",this.getUser());
            pane_insert.Text = ui.Text("general", "insert",this.getUser()) + " " + ui.Text("general", "macro",this.getUser());

			if (Request["macroID"] != null || Request["macroAlias"] != null) 
			{
				// Put user code to initialize the page here
				cms.businesslogic.macro.Macro m;
				if (helper.Request("macroID") != "")
					m = new umbraco.cms.businesslogic.macro.Macro(int.Parse(helper.Request("macroID")));
				else
					m = cms.businesslogic.macro.Macro.GetByAlias(helper.Request("macroAlias"));

				String macroAssembly = "";
				String macroType = "";

				foreach (cms.businesslogic.macro.MacroProperty mp in m.Properties) {
		
					macroAssembly = mp.Type.Assembly;
					macroType = mp.Type.Type;
					try 
					{

                        Assembly assembly = Assembly.LoadFrom( IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

						Type type = assembly.GetType(macroAssembly+"."+macroType);
						interfaces.IMacroGuiRendering typeInstance = Activator.CreateInstance(type) as interfaces.IMacroGuiRendering;
						if (typeInstance != null) 
						{
							Control control = Activator.CreateInstance(type) as Control;	
							control.ID = mp.Alias;
							if (Request[mp.Alias] != null) 
							{
								if (Request[mp.Alias] != "") 
								{
									type.GetProperty("Value").SetValue(control, Convert.ChangeType(Request[mp.Alias], type.GetProperty("Value").PropertyType), null);
								}
							}

							// register alias
                            umbraco.uicontrols.PropertyPanel pp = new umbraco.uicontrols.PropertyPanel();
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
					macroRenderings = SqlHelper.ExecuteReader("select macroAlias, macroName from cmsMacro where macroUseInEditor = 1 order by macroName");
				else
					macroRenderings = SqlHelper.ExecuteReader("select macroAlias, macroName from cmsMacro order by macroName");
				
				macroAlias.DataSource = macroRenderings;
				macroAlias.DataValueField = "macroAlias";
				macroAlias.DataTextField = "macroName";
				macroAlias.DataBind();
				macroRenderings.Close();

			}




		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion
	}
}
