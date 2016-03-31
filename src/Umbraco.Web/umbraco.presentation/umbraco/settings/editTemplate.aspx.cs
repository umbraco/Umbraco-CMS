using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.template;
using umbraco.uicontrols;
using Umbraco.Web.UI.Pages;

namespace umbraco.cms.presentation.settings
{
	/// <summary>
	/// Summary description for editTemplate.
	/// </summary>
	public partial class editTemplate : UmbracoEnsuredPage
	{
		private Template _template;
        public MenuButton SaveButton;

		public editTemplate()
		{
			CurrentApp = Constants.Applications.Settings.ToString();
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
				MasterTemplate.Items.Add(new ListItem(Services.TextService.Localize("none"), "0"));
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
                    .SetActiveTreeType(Constants.Trees.Templates)
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

		    var editor = Panel1.NewTabPage(Services.TextService.Localize("template"));
            editor.Controls.Add(Pane8);

            var props = Panel1.NewTabPage(Services.TextService.Localize("properties"));
            props.Controls.Add(Pane7);

            SaveButton = Panel1.Menu.NewButton();
            SaveButton.Text = Services.TextService.Localize("save");
            SaveButton.ButtonType = MenuButtonType.Primary;
            SaveButton.ID = "save";
            SaveButton.CssClass = "client-side";

			Panel1.Text = Services.TextService.Localize("edittemplate");
			pp_name.Text = Services.TextService.Localize("name");
            pp_alias.Text = Services.TextService.Localize("alias");
            pp_masterTemplate.Text = Services.TextService.Localize("mastertemplate");


			// Editing buttons
            MenuIconI umbField = editorSource.Menu.NewIcon();
			umbField.ImageURL = SystemDirectories.Umbraco + "/images/editor/insField.gif";
			umbField.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
					editorSource.ClientID + "&tagName=UMBRACOGETDATA", Services.TextService.Localize("template/insertPageField"), 640, 550);
			umbField.AltText = Services.TextService.Localize("template/insertPageField");


			// TODO: Update icon
            MenuIconI umbDictionary = editorSource.Menu.NewIcon();
			umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
			umbDictionary.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
					editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY", Services.TextService.Localize("template/insertDictionaryItem"),
					640, 550);
			umbDictionary.AltText = "Insert umbraco dictionary item";

		    editorSource.Menu.NewElement("div", "splitButtonMacroPlaceHolder", "sbPlaceHolder", 40);

			if (UmbracoConfig.For.UmbracoSettings().Templates.UseAspNetMasterPages)
			{
			    MenuIconI umbContainer = editorSource.Menu.NewIcon();
				umbContainer.ImageURL = SystemDirectories.Umbraco + "/images/editor/masterpagePlaceHolder.gif";
				umbContainer.AltText = Services.TextService.Localize("template/insertContentAreaPlaceHolder");
				umbContainer.OnClickCommand =
					ClientTools.Scripts.OpenModalWindow(
						IOHelper.ResolveUrl(SystemDirectories.Umbraco) +
						"/dialogs/insertMasterpagePlaceholder.aspx?&id=" + _template.Id,
						Services.TextService.Localize("template/insertContentAreaPlaceHolder"), 470, 320);

                MenuIconI umbContent = editorSource.Menu.NewIcon();
				umbContent.ImageURL = SystemDirectories.Umbraco + "/images/editor/masterpageContent.gif";
				umbContent.AltText = Services.TextService.Localize("template/insertContentArea");
				umbContent.OnClickCommand =
					ClientTools.Scripts.OpenModalWindow(
						IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/insertMasterpageContent.aspx?id=" +
						_template.Id, Services.TextService.Localize("template/insertContentArea"), 470, 300);
			}


			//Spit button
            editorSource.Menu.InsertSplitter();
            editorSource.Menu.NewElement("div", "splitButtonPlaceHolder", "sbPlaceHolder", 40);

			// Help
            editorSource.Menu.InsertSplitter();

            MenuIconI helpIcon = editorSource.Menu.NewIcon();
			helpIcon.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=" +
					_template.Alias, Services.TextService.Localize("template/quickGuide"), 600, 580);
			helpIcon.ImageURL = SystemDirectories.Umbraco + "/images/editor/help.png";
			helpIcon.AltText = Services.TextService.Localize("template/quickGuide");
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
		    ;
		    var macroRenderings =
		        Services.MacroService.GetAll()
		            .Select(x => new TempMacroClass()
		            {
		                id = x.Id,
		                macroAlias = x.Alias,
		                macroName = x.Name
		            });

			rpt_macros.DataSource = macroRenderings;
			rpt_macros.DataBind();
		}

	    private class TempMacroClass
	    {
	        public int id { get; set; }
            public string macroAlias { get; set; }
            public string macroName { get; set; }
	    }

		public string DoesMacroHaveSettings(string macroId)
		{
			if (
				DatabaseContext.Database.ExecuteScalar<int>(string.Format("select 1 from cmsMacroProperty where macro = {0}", macroId)) ==
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
		protected global::umbraco.uicontrols.TabView Panel1;

		/// <summary>
		/// Pane7 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane7;
        protected global::umbraco.uicontrols.Pane Pane8;

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