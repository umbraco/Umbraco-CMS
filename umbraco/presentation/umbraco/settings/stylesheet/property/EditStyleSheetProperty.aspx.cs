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
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.settings.stylesheet
{
	/// <summary>
	/// Summary description for EditStyleSheetProperty.
	/// </summary>
	public partial class EditStyleSheetProperty : BasePages.UmbracoEnsuredPage
	{
	    public EditStyleSheetProperty()
	    {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();
	        
	    }

		private cms.businesslogic.web.StylesheetProperty stylesheetproperty;
		private DropDownList ddl = new DropDownList();
		
		protected void Page_Load(object sender, System.EventArgs e)
		{
			stylesheetproperty = new cms.businesslogic.web.StylesheetProperty(int.Parse(Request.QueryString["id"]));
            Panel1.Text = ui.Text("stylesheet", "editstylesheetproperty", base.getUser());

            if (!IsPostBack) {
                stylesheetproperty.RefreshFromFile();
                NameTxt.Text = stylesheetproperty.Text;
                Content.Text = stylesheetproperty.value;
                AliasTxt.Text = stylesheetproperty.Alias;

				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadStylesheetProperty>().Tree.Alias)
					.SyncTree(helper.Request("id"), false);
            }		
		
			ImageButton bt = Panel1.Menu.NewImageButton();
			bt.Click += new System.Web.UI.ImageClickEventHandler(save_click);
			bt.ImageUrl = UmbracoPath +"/images/editor/save.gif";
			bt.AlternateText = ui.Text("save");
			setupPreView();
		}

		protected override void OnPreRender(EventArgs e)
		{
            prStyles.Attributes["style"] = stylesheetproperty.value;
			
            base.OnPreRender (e);
		}

		private void setupPreView() 
		{
            prStyles.Attributes["style"] = stylesheetproperty.value;
		}

		private void save_click(object sender, System.Web.UI.ImageClickEventArgs e) 
		{
			stylesheetproperty.value = Content.Text;
			stylesheetproperty.Text = NameTxt.Text;
			stylesheetproperty.Alias = AliasTxt.Text;

			try 
			{
				stylesheetproperty.StyleSheet().saveCssToFile();
			} 
			catch {}
			this.speechBubble(speechBubbleIcon.save,ui.Text("speechBubbles", "editStylesheetPropertySaved", base.getUser()),"");
			setupPreView();

            stylesheetproperty.Save();
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
			Content.TextMode = TextBoxMode.MultiLine;
			Content.Height = 250;
			Content.Width = 300;
			
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
