using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.presentation.Trees;


namespace umbraco.cms.presentation.settings
{
	/// <summary>
	/// Summary description for editTemplate.
	/// </summary>
	public partial class editTemplate : BasePages.UmbracoEnsuredPage
	{
		private cms.businesslogic.template.Template _template;

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }


		protected void Page_Load(object sender, System.EventArgs e)
		{
            MasterTemplate.Attributes.Add("onchange", "changeMasterPageFile()");

            if (!IsPostBack) {
               

				MasterTemplate.Items.Add(new ListItem(ui.Text("none"),"0"));
				foreach (cms.businesslogic.template.Template t in cms.businesslogic.template.Template.GetAllAsList()) 
				{
					if (t.Id != _template.Id) 
					{
						ListItem li = new ListItem(t.Text,t.Id.ToString());

                        li.Attributes.Add("id", t.Alias.Replace(" ", ""));

						if (t.Id == _template.MasterTemplate) 
						{
							try 
							{
								li.Selected = true;
							}
							catch {}
						}
						MasterTemplate.Items.Add(li);
					}
				}

				NameTxt.Text = _template.GetRawText();
				AliasTxt.Text = _template.Alias;
                editorSource.Text = _template.Design;


				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadTemplates>().Tree.Alias)
					.SyncTree(_template.Id.ToString(), false);
              }			
		}

		

		override protected void OnInit(EventArgs e)
		{
            _template = new cms.businesslogic.template.Template(int.Parse(Request.QueryString["templateID"]));
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
			Panel1.hasMenu = true;

            uicontrols.MenuIconI save = Panel1.Menu.NewIcon();
            save.ImageURL = GlobalSettings.Path + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = ui.Text("save");
	
			Panel1.Text = ui.Text("edittemplate");
            pp_name.Text = ui.Text("name", base.getUser());
            pp_alias.Text = ui.Text("alias", base.getUser());
            pp_masterTemplate.Text = ui.Text("mastertemplate", base.getUser());

			// Editing buttons
			Panel1.Menu.InsertSplitter();
			uicontrols.MenuIconI umbField = Panel1.Menu.NewIcon();
			umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
            umbField.OnClickCommand = "top.openModal('dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDATA', '" + ui.Text("template", "insertPageField") + "', 590, 640);";
            umbField.AltText = ui.Text("template", "insertPageField");

            // TODO: Update icon
            uicontrols.MenuIconI umbDictionary = Panel1.Menu.NewIcon();
            umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
            umbDictionary.OnClickCommand = "top.openModal('dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY', '" + ui.Text("template", "insertDictionaryItem") + "', 590, 640);";
            //umbDictionary.OnClickCommand = "umbracoInsertField(document.forms[0].TemplateBody, 'umbracoField', 'UMBRACOGETDICTIONARY','felt', 640, 650, 'dialogs/');";
            //umbDictionary.OnClickCommand = "top.openModal('dialogs/editMacro.aspx?objectId=" + editorSource.ClientID + "', 'Insert dictionary item', 550, 470);";
            umbDictionary.AltText = "Insert umbraco dictionary item";
            
            uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
            umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
            umbMacro.AltText = ui.Text("template", "insertMacro");
            umbMacro.OnClickCommand = "top.openModal('dialogs/editMacro.aspx?objectId=" + editorSource.ClientID + "', '" + ui.Text("template", "insertMacro") + "', 550, 470);";
            //umbMacro.OnClickCommand = "umbracoTemplateInsertMacro();";

            if (UmbracoSettings.UseAspNetMasterPages) {

                Panel1.Menu.InsertSplitter();

                uicontrols.MenuIconI umbContainer = Panel1.Menu.NewIcon();
                umbContainer.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
                umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
                //umbContainer.OnClickCommand = "umbracoTemplateInsertMasterPageContentContainer();";
                umbContainer.OnClickCommand = "top.openModal('dialogs/insertMasterpagePlaceholder.aspx?&id=" + _template.Id.ToString() + "', '" + ui.Text("template", "insertContentAreaPlaceHolder") + "', 340, 470);";
                
                uicontrols.MenuIconI umbContent = Panel1.Menu.NewIcon();
                umbContent.ImageURL = UmbracoPath + "/images/editor/masterpageContent.gif";
                umbContent.AltText = ui.Text("template", "insertContentArea");
                umbContent.OnClickCommand = "top.openModal('dialogs/insertMasterpageContent.aspx?id=" + _template.Id.ToString() + "', '" + ui.Text("template", "insertContentArea") + "', 320, 470);";
            }

			// Help
			Panel1.Menu.InsertSplitter();

			uicontrols.MenuIconI helpIcon = Panel1.Menu.NewIcon();
            helpIcon.OnClickCommand = "top.openModal('settings/modals/showumbracotags.aspx?alias=" + _template.Alias + "', '" + ui.Text("template", "quickGuide") + "', 450, 750);";
			helpIcon.ImageURL = UmbracoPath + "/images/editor/help.png";
            helpIcon.AltText = ui.Text("template", "quickGuide");
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}

	}

}
