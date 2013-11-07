using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.template;
using umbraco.cms.presentation.Trees;
using umbraco.DataLayer;
using Umbraco.Core.IO;
using umbraco.uicontrols;
using System.Linq;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.presentation.settings
{
	/// <summary>
	/// Summary description for editTemplate.
	/// </summary>
	public partial class editTemplate : UmbracoEnsuredPage
	{
		private Template _template;

		public editTemplate()
		{
			CurrentApp = DefaultApps.settings.ToString();
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			ScriptManager.GetCurrent(Page).Services.Add(
				new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices + "/codeEditorSave.asmx")));
			ScriptManager.GetCurrent(Page).Services.Add(
                new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices + "/legacyAjaxCalls.asmx")));
		}

        protected string TemplateTreeSyncPath { get; private set; }

		protected void Page_Load(object sender, EventArgs e)
		{
			MasterTemplate.Attributes.Add("onchange", "changeMasterPageFile()");

		    TemplateTreeSyncPath = "-1,init," + _template.Path.Replace("-1,", "");

			if (!IsPostBack)
			{
				MasterTemplate.Items.Add(new ListItem(ui.Text("none"), "0"));
				foreach (Template t in Template.GetAllAsList())
				{
					if (t.Id != _template.Id)
					{
						var li = new ListItem(t.Text, t.Id.ToString());
						li.Attributes.Add("id", t.Alias.Replace(" ", ""));
						MasterTemplate.Items.Add(li);
					}
				}

				NameTxt.Text = _template.GetRawText();
				AliasTxt.Text = _template.Alias;
				editorSource.Text = _template.Design;

				try
				{
					if (_template.MasterTemplate > 0)
						MasterTemplate.SelectedValue = _template.MasterTemplate.ToString();
				}
				catch (Exception ex)
				{
				}

				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadTemplates>().Tree.Alias)
                    .SyncTree(TemplateTreeSyncPath, false);

				LoadScriptingTemplates();
				LoadMacros();
			}
		}

		protected override void OnInit(EventArgs e)
		{
			_template = new Template(int.Parse(Request.QueryString["templateID"]));
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
			Panel1.hasMenu = true;

			MenuIconI save = Panel1.Menu.NewIcon();
			save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
			save.OnClickCommand = "doSubmit()";
			save.AltText = ui.Text("save");
			save.ID = "save";

			Panel1.Text = ui.Text("edittemplate");
			pp_name.Text = ui.Text("name", base.getUser());
			pp_alias.Text = ui.Text("alias", base.getUser());
			pp_masterTemplate.Text = ui.Text("mastertemplate", base.getUser());

			// Editing buttons
			Panel1.Menu.InsertSplitter();
			MenuIconI umbField = Panel1.Menu.NewIcon();
			umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
			umbField.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
					editorSource.ClientID + "&tagName=UMBRACOGETDATA", ui.Text("template", "insertPageField"), 640, 550);
			umbField.AltText = ui.Text("template", "insertPageField");

			// TODO: Update icon
			MenuIconI umbDictionary = Panel1.Menu.NewIcon();
			umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
			umbDictionary.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
					editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY", ui.Text("template", "insertDictionaryItem"),
					640, 550);
			umbDictionary.AltText = "Insert umbraco dictionary item";

			//uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
			//umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
			//umbMacro.AltText = ui.Text("template", "insertMacro");
			//umbMacro.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/dialogs/editMacro.aspx?objectId=" + editorSource.ClientID, ui.Text("template", "insertMacro"), 470, 530);

			Panel1.Menu.NewElement("div", "splitButtonMacroPlaceHolder", "sbPlaceHolder", 40);

			if (UmbracoSettings.UseAspNetMasterPages)
			{
				Panel1.Menu.InsertSplitter();

				MenuIconI umbContainer = Panel1.Menu.NewIcon();
				umbContainer.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
				umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
				umbContainer.OnClickCommand =
					ClientTools.Scripts.OpenModalWindow(
						IOHelper.ResolveUrl(SystemDirectories.Umbraco) +
						"/dialogs/insertMasterpagePlaceholder.aspx?&id=" + _template.Id,
						ui.Text("template", "insertContentAreaPlaceHolder"), 470, 320);

				MenuIconI umbContent = Panel1.Menu.NewIcon();
				umbContent.ImageURL = UmbracoPath + "/images/editor/masterpageContent.gif";
				umbContent.AltText = ui.Text("template", "insertContentArea");
				umbContent.OnClickCommand =
					ClientTools.Scripts.OpenModalWindow(
						IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/insertMasterpageContent.aspx?id=" +
						_template.Id, ui.Text("template", "insertContentArea"), 470, 300);
			}


			//Spit button
			Panel1.Menu.InsertSplitter();
			Panel1.Menu.NewElement("div", "splitButtonPlaceHolder", "sbPlaceHolder", 40);

			if (Skinning.StarterKitGuid(_template.Id).HasValue)
			{
				Panel1.Menu.InsertSplitter();
				MenuIconI umbContainer = Panel1.Menu.NewIcon();
				umbContainer.ImageURL = UmbracoPath + "/images/editor/skin.gif";
				umbContainer.AltText = ui.Text("template", "modifyTemplateSkin");
				//umbContainer.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/dialogs/TemplateSkinning.aspx?&id=" + _template.Id.ToString(), ui.Text("template", "modifyTemplateSkin"), 570, 420);
				umbContainer.OnClickCommand = "window.open('" + GlobalSettings.Path + "/canvas.aspx?redir=" +
											  ResolveUrl("~/") + "&umbSkinning=true&umbSkinningConfigurator=true" +
											  "','canvas')";
			}

			// Help
			Panel1.Menu.InsertSplitter();

			MenuIconI helpIcon = Panel1.Menu.NewIcon();
			helpIcon.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=" +
					_template.Alias, ui.Text("template", "quickGuide"), 600, 580);
			helpIcon.ImageURL = UmbracoPath + "/images/editor/help.png";
			helpIcon.AltText = ui.Text("template", "quickGuide");
		}


		private void LoadScriptingTemplates()
		{
			string path = SystemDirectories.Umbraco + "/scripting/templates/cshtml/";
			string abPath = IOHelper.MapPath(path);

			var files = new List<KeyValuePair<string, string>>();

			if (Directory.Exists(abPath))
			{
				string extension = ".cshtml";

				foreach (FileInfo fi in new DirectoryInfo(abPath).GetFiles("*" + extension))
				{
					string filename = Path.GetFileName(fi.FullName);

					files.Add(new KeyValuePair<string, string>(
								  filename,
                                  filename.Replace(extension, "").SplitPascalCasing().ToFirstUpperInvariant()
								  ));
				}
			}

			rpt_codeTemplates.DataSource = files;
			rpt_codeTemplates.DataBind();
		}

        private void LoadMacros()
        {
            var macroRenderings =
                    ApplicationContext.Current.DatabaseContext.Database.Fetch<MacroDto>("select id, macroAlias, macroName from cmsMacro order by macroName"); 

	        rpt_macros.DataSource = macroRenderings;
	        rpt_macros.DataBind();
        }

		public string DoesMacroHaveSettings(string macroId)
		{
			if (
                ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                            string.Format("select 1 from cmsMacroProperty where macro = {0}", macroId)) == 
				1)
				return "1";
			else
				return "0";
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// CssInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;

		/// <summary>
		/// JsInclude control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude;

		/// <summary>
		/// Panel1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoPanel Panel1;

		/// <summary>
		/// Pane7 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane7;

		/// <summary>
		/// pp_name control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_name;

		/// <summary>
		/// NameTxt control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox NameTxt;

		/// <summary>
		/// pp_alias control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_alias;

		/// <summary>
		/// AliasTxt control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox AliasTxt;

		/// <summary>
		/// pp_masterTemplate control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_masterTemplate;

		/// <summary>
		/// MasterTemplate control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.DropDownList MasterTemplate;

		/// <summary>
		/// pp_source control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_source;

		/// <summary>
		/// editorSource control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.CodeArea editorSource;

		/// <summary>
		/// rpt_codeTemplates control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Repeater rpt_codeTemplates;

		/// <summary>
		/// rpt_macros control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Repeater rpt_macros;
	}
}