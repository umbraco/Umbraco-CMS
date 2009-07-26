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
using umbraco.BusinessLogic;
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.settings.stylesheet
{
	/// <summary>
	/// Summary description for editstylesheet.
	/// </summary>
	public partial class editstylesheet : BasePages.UmbracoEnsuredPage
	{
		private cms.businesslogic.web.StyleSheet stylesheet;

		protected void Page_Load(object sender, System.EventArgs e)
		{

            if(!IsPostBack)
            {
                // editor source
				
				if (!UmbracoSettings.ScriptDisableEditor)
				{
					// TODO: Register the some script editor js file if you can find a good one.
				}

				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadStylesheets>().Tree.Alias)
					.SyncTree(helper.Request("id"), false);
            }

             uicontrols.MenuIconI save = Panel1.Menu.NewIcon();
             save.ImageURL = GlobalSettings.Path + "/images/editor/save.gif";
             save.OnClickCommand = "doSubmit()";
             save.AltText = "Save stylesheet";

             Panel1.Text = ui.Text("stylesheet", "editstylesheet", base.getUser());
             pp_name.Text = ui.Text("name", base.getUser());
             pp_path.Text = ui.Text("path", base.getUser());

			stylesheet = new cms.businesslogic.web.StyleSheet(int.Parse(Request.QueryString["id"]));
			string appPath = Request.ApplicationPath;
			if (appPath == "/") 
				appPath = "";
            lttPath.Text = "<a target='_blank' href='" + appPath + "/css/" + stylesheet.Text + ".css'>" + appPath + "/css/" + stylesheet.Text + ".css</a>";
			

			if (!IsPostBack) 
			{
				NameTxt.Text = stylesheet.Text;
                editorSource.Text = stylesheet.Content;
			}
		}

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
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
