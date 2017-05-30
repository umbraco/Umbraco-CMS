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
using System.Text;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Web;
using umbraco.DataLayer;


namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for insertMacro.
	/// </summary>
	public partial class editMacro : BasePages.UmbracoEnsuredPage
	{
		protected Button Button1;

		protected cms.businesslogic.macro.Macro MacroObject { get; private set; }

		public string _macroAlias = "";

		protected void renderProperties(object sender, EventArgs e)
		{
			if (umb_macroAlias.SelectedValue != "")
			{
				AskForProperties(umb_macroAlias.SelectedValue);
			}
			else
			{
				pl_insert.Visible = true;
				pl_edit.Visible = false;
			}
		}

		private void AskForProperties(string alias)
		{
			pl_edit.Visible = true;
			pl_insert.Visible = false;

			MacroObject = cms.businesslogic.macro.Macro.GetByAlias(alias);

			_macroAlias = MacroObject.Alias;


			//If no properties, we will exit now...
			if (MacroObject.Properties.Length == 0)
			{
				//var noProps = new Literal();
				//noProps.Text = "<script type='text/javascript'>Umbraco.Dialogs.EditMacro.getInstance().updateMacro()</script>";
				//macroProperties.Controls.Add(noProps);
			}
			else
			{
				//if we have properties, we'll render the controls for them...
				foreach (cms.businesslogic.macro.MacroProperty mp in MacroObject.Properties)
				{
					var macroAssembly = mp.Type.Assembly;
					var macroType = mp.Type.Type;
					try
					{

						Assembly assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

						Type type = assembly.GetType(macroAssembly + "." + macroType);
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
							var pp = new umbraco.uicontrols.PropertyPanel();
							pp.Text = mp.Name;
							pp.Controls.Add(control);
							macroProperties.Controls.Add(pp);

							pp.Controls.Add(new LiteralControl("<script type=\"text/javascript\">Umbraco.Dialogs.EditMacro.getInstance().registerAlias('" + control.ClientID + "','" + mp.Alias + "'); </script>\n"));



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
		protected void Page_Load(object sender, System.EventArgs e)
		{

			if (!Page.IsPostBack)
			{
				if (!string.IsNullOrEmpty(Request["alias"]))
				{
					AskForProperties(Request["alias"]);
				}
				else
				{
				    string query;
				    if (Request.GetItemAsString("editor") != "")
				        query = "select macroAlias, macroName from cmsMacro where macroUseInEditor = 1 order by macroName";
				    else
				        query = "select macroAlias, macroName from cmsMacro order by macroName";

                    using (var sqlHelper = BusinessLogic.Application.SqlHelper)
                    {
                        using (IRecordsReader macroRenderings = sqlHelper.ExecuteReader(query))
                        {
                            umb_macroAlias.DataSource = macroRenderings;
                            umb_macroAlias.DataValueField = "macroAlias";
                            umb_macroAlias.DataTextField = "macroName";
                            umb_macroAlias.DataBind();
                        }
                    }
				}
			}

		}		

		/// <summary>
		/// pl_edit control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Panel pl_edit;

		/// <summary>
		/// pane_edit control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane pane_edit;

		/// <summary>
		/// macroProperties control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.PlaceHolder macroProperties;

		/// <summary>
		/// pl_insert control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Panel pl_insert;

		/// <summary>
		/// pane_insert control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane pane_insert;

		/// <summary>
		/// pp_chooseMacro control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_chooseMacro;

		/// <summary>
		/// umb_macroAlias control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.ListBox umb_macroAlias;

		/// <summary>
		/// bt_insert control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Button bt_insert;

		/// <summary>
		/// renderHolder control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.PlaceHolder renderHolder;


	}
}
