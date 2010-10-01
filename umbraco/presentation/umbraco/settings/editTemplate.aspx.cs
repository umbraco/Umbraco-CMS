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
using umbraco.IO;
using umbraco.cms.businesslogic.skinning;


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

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference( IOHelper.ResolveUrl( SystemDirectories.Webservices + "/codeEditorSave.asmx") ));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference( IOHelper.ResolveUrl( SystemDirectories.Webservices + "/legacyAjaxCalls.asmx") ));
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
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
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
            umbField.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDATA", ui.Text("template", "insertPageField"), 640, 550);
            umbField.AltText = ui.Text("template", "insertPageField");

            // TODO: Update icon
            uicontrols.MenuIconI umbDictionary = Panel1.Menu.NewIcon();
            umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
            umbDictionary.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY", ui.Text("template", "insertDictionaryItem"), 640, 550);
            umbDictionary.AltText = "Insert umbraco dictionary item";
            
            uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
            umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
            umbMacro.AltText = ui.Text("template", "insertMacro");
            umbMacro.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/editMacro.aspx?objectId=" + editorSource.ClientID, ui.Text("template", "insertMacro"), 470, 530);

            if (UmbracoSettings.UseAspNetMasterPages) {

                Panel1.Menu.InsertSplitter();

                uicontrols.MenuIconI umbContainer = Panel1.Menu.NewIcon();
                umbContainer.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
                umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
                umbContainer.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/insertMasterpagePlaceholder.aspx?&id=" + _template.Id.ToString(), ui.Text("template", "insertContentAreaPlaceHolder"), 470, 320);
                
                uicontrols.MenuIconI umbContent = Panel1.Menu.NewIcon();
                umbContent.ImageURL = UmbracoPath + "/images/editor/masterpageContent.gif";
                umbContent.AltText = ui.Text("template", "insertContentArea");
                umbContent.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/insertMasterpageContent.aspx?id=" + _template.Id.ToString(), ui.Text("template", "insertContentArea"), 470, 300);
            }

            if (Skinning.StarterKitGuid(_template.Id).HasValue)
            {
                Panel1.Menu.InsertSplitter();
                uicontrols.MenuIconI umbContainer = Panel1.Menu.NewIcon();
                umbContainer.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
                umbContainer.AltText = ui.Text("template", "modifyTemplateSkin");
                //umbContainer.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/TemplateSkinning.aspx?&id=" + _template.Id.ToString(), ui.Text("template", "modifyTemplateSkin"), 570, 420);
                umbContainer.OnClickCommand = "parent.window.location = '" + GlobalSettings.Path + "/canvas.aspx?redir=" + this.ResolveUrl("~/") + "&umbSkinning=true&umbSkinningConfigurator=true" + "'";
            }


			// Help
			Panel1.Menu.InsertSplitter();

			uicontrols.MenuIconI helpIcon = Panel1.Menu.NewIcon();
            helpIcon.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=" + _template.Alias, ui.Text("template", "quickGuide"), 600, 580);
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
