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
using umbraco.DataLayer;
using umbraco.IO;


namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for insertMacro.
	/// </summary>
	public partial class editMacro : BasePages.UmbracoEnsuredPage
	{
		protected System.Web.UI.WebControls.Button Button1;

		private cms.businesslogic.macro.Macro m;

        public string _macroAlias = "";


        protected void renderProperties(object sender, EventArgs e) {
            if (umb_macroAlias.SelectedValue != "") {
                pl_edit.Visible = true;
                pl_insert.Visible = false;

                m = cms.businesslogic.macro.Macro.GetByAlias(umb_macroAlias.SelectedValue);

                String macroAssembly = "";
				String macroType = "";

                _macroAlias = m.Alias;
                

                //If no properties, we will exit now...
                if (m.Properties.Length == 0) {
                    Literal noProps = new Literal();
                    noProps.Text = "<script type='text/javascript'>updateMacro()</script>";
                    macroProperties.Controls.Add(noProps);
                } else {
                    //if we have properties, we'll render the controls for them...
                    foreach (cms.businesslogic.macro.MacroProperty mp in m.Properties) {
                        macroAssembly = mp.Type.Assembly;
                        macroType = mp.Type.Type;
                        try {

                            Assembly assembly = Assembly.LoadFrom(IOHelper.MapPath(SystemDirectories.Bin + "/" + macroAssembly + ".dll"));

                            Type type = assembly.GetType(macroAssembly + "." + macroType);
                            interfaces.IMacroGuiRendering typeInstance = Activator.CreateInstance(type) as interfaces.IMacroGuiRendering;
                            if (typeInstance != null) {
                                Control control = Activator.CreateInstance(type) as Control;
                                control.ID = mp.Alias;
                                
                                if (!IsPostBack) {
                                    if (Request["umb_" + mp.Alias] != null) {
                                        if (Request["umb_" + mp.Alias] != "") {
                                            type.GetProperty("Value").SetValue(control, Convert.ChangeType(Request["umb_" + mp.Alias], type.GetProperty("Value").PropertyType), null);
                                        }
                                    }
                                }

                                // register alias
                                umbraco.uicontrols.PropertyPanel pp = new umbraco.uicontrols.PropertyPanel();
                                pp.Text = mp.Name;
                                pp.Controls.Add(control);
                                macroProperties.Controls.Add(pp);

                                pp.Controls.Add(new LiteralControl("<script type=\"text/javascript\"> registerAlias('" + control.ClientID + "','" + mp.Alias + "'); </script>\n"));
                                


                            } else {
                                Trace.Warn("umbEditContent", "Type doesn't exist or is not umbraco.interfaces.DataFieldI ('" + macroAssembly + "." + macroType + "')");
                            }

                        } catch (Exception fieldException) {
                            Trace.Warn("umbEditContent", "Error creating type '" + macroAssembly + "." + macroType + "'", fieldException);
                        }
                    }
                } 
            }else {
                pl_insert.Visible = true;
                pl_edit.Visible = false;
            }
        }


		protected void Page_Load(object sender, System.EventArgs e)
		{

            if (!Page.IsPostBack)
            {
                IRecordsReader macroRenderings;
                if (helper.Request("editor") != "")
                    macroRenderings = SqlHelper.ExecuteReader("select macroAlias, macroName from cmsMacro where macroUseInEditor = 1 order by macroName");
                else
                    macroRenderings = SqlHelper.ExecuteReader("select macroAlias, macroName from cmsMacro order by macroName");

                umb_macroAlias.DataSource = macroRenderings;
                umb_macroAlias.DataValueField = "macroAlias";
                umb_macroAlias.DataTextField = "macroName";
                umb_macroAlias.DataBind();
                macroRenderings.Close();
            }
            else
            {

                ScriptManager.RegisterOnSubmitStatement(Page, Page.GetType(), "myHandlerKey", "updateMacro()");
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

        /*
		private void renderMacro_Click(object sender, EventArgs e)
		{
			int pageID = int.Parse(helper.Request("umbPageId"));
			string macroAttributes = "macroAlias=\"" + m.Alias + "\"";
			Guid pageVersion = new Guid(helper.Request("umbVersionId"));

			Hashtable attributes = new Hashtable();
			attributes.Add("macroAlias", m.Alias);
			umbraco.macro mRender = new macro(m.Id);
			foreach(Control c in macroProperties.Controls) 
			{
				try 
				{
					interfaces.IMacroGuiRendering ic = (interfaces.IMacroGuiRendering) c;
					attributes.Add(c.ID, ic.Value);
					macroAttributes += " " + c.ID + "=\"" + ic.Value + "\"";
				} 
				catch {}
			}

			// document this, for gods sake!
			System.Web.HttpContext.Current.Items["macrosAdded"] = 0;
			System.Web.HttpContext.Current.Items["pageID"] = pageID.ToString();


			umbraco.page p = new page(pageID, pageVersion);

			string div = macro.renderMacroStartTag(attributes, pageID, pageVersion);

			string macroContent = macro.MacroContentByHttp(pageID, pageVersion, attributes).Replace("\\", "\\\\").Replace("'", "\\'").Replace("/", "\\/");
			
            if (macroContent.Length > 0 && macroContent.ToLower().IndexOf("<script") > -1)
				macroContent = "<b>Macro rendering contains script code</b><br/>This macro won\\'t be rendered in the editor because it contains script code. It will render correct during runtime.";
			div += macroContent;
			div += macro.renderMacroEndTag();

			ClientScript.RegisterStartupScript(this.GetType(), "postbackScript", "<script>\n parent.opener.umbracoEditMacroDo('" + macroAttributes + "', '" + m.Name.Replace("'", "\'") + "', '" + div + "');\n</script>");
            ClientScript.RegisterStartupScript(this.GetType(), "postbackScriptWindowClose", "<script>\n setTimeout('window.close()',300);\n</script>");
			

		}
	    */
    }
}
