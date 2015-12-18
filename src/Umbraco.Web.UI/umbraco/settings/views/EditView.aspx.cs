using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Trees;
using Umbraco.Web.UI.Controls;
using umbraco;
using umbraco.BasePages;
using umbraco.cms.businesslogic.template;
using umbraco.cms.helpers;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using umbraco.uicontrols;

namespace Umbraco.Web.UI.Umbraco.Settings.Views
{
	public partial class EditView : global::umbraco.BasePages.UmbracoEnsuredPage
	{
		private Template _template;
		public MenuButton SaveButton;

		public EditView()
		{
			CurrentApp = global::umbraco.BusinessLogic.DefaultApps.settings.ToString();
		}

		/// <summary>
		/// The type of MVC/Umbraco view the editor is editing
		/// </summary>
		public enum ViewEditorType
		{
			Template,
			PartialView,
            PartialViewMacro
		}

		/// <summary>
		/// Returns the type of view being edited
		/// </summary>
		protected ViewEditorType EditorType
		{
		    get
		    {
		        if (_template != null) return ViewEditorType.Template;
                if (Request.QueryString["treeType"].IsNullOrWhiteSpace() == false && Request.QueryString["treeType"].InvariantEquals("partialViewMacros")) return ViewEditorType.PartialViewMacro;
		        return ViewEditorType.PartialView;
		    }
		}

        protected string TemplateTreeSyncPath { get; private set; }
	    
        /// <summary>
        /// This view is shared between different trees so we'll look for the query string
        /// </summary>
        protected string CurrentTreeType
	    {
	        get
	        {
	            if (Request.QueryString["treeType"].IsNullOrWhiteSpace())
	            {
	                return TreeDefinitionCollection.Instance.FindTree<PartialViewsTree>().Tree.Alias;
	            }
	            return Request.CleanForXss("treeType");
	        }
	    }

		/// <summary>
		/// Returns the original file name that the editor was loaded with
		/// </summary>
		/// <remarks>
		/// this is used for editing a partial view
		/// </remarks>
		protected string OriginalFileName { get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!IsPostBack)
			{

				//configure screen for editing a template
				if (_template != null)
				{
					MasterTemplate.Items.Add(new ListItem(ui.Text("none"), "0"));
					var selectedTemplate = string.Empty;

					foreach (var t in Template.GetAllAsList())
					{
						if (t.Id == _template.Id) continue;

						var li = new ListItem(t.Text, t.Id.ToString(CultureInfo.InvariantCulture));
						li.Attributes.Add("id", t.Alias.Replace(" ", "") + ".cshtml");
						MasterTemplate.Items.Add(li);
					}

					try
					{
						if (_template.MasterTemplate > 0)
							MasterTemplate.SelectedValue = _template.MasterTemplate.ToString(CultureInfo.InvariantCulture);
					}
					catch (Exception ex)
					{
                        LogHelper.Error<EditView>("An error occurred setting a master template id", ex);
					}

					MasterTemplate.SelectedValue = selectedTemplate;
					NameTxt.Text = _template.GetRawText();
					AliasTxt.Text = _template.Alias;
					editorSource.Text = _template.Design;
				    PathPrefix.Visible = false;
				}
				else
				{
					//configure editor for editing a file....

					NameTxt.Text = OriginalFileName;
				    var svce = ApplicationContext.Current.Services.FileService;
                    var file = EditorType == ViewEditorType.PartialView
				        ? svce.GetPartialView(OriginalFileName)
                        : svce.GetPartialViewMacro(OriginalFileName);
				    editorSource.Text = file.Content;

                    const string prefixFormat = "<span style=\"display: inline-block; height: 20px; line-height: 20px; margin-bottom: 0px; padding: 4px 6px;\">{0}</span>";
                    PathPrefix.Text = string.Format(prefixFormat, EditorType == ViewEditorType.PartialView
				        ? "Partials/"
				        : "MacroPartials/");
				}							
			}
            
            ClientTools
                .SetActiveTreeType(CurrentTreeType)
                .SyncTree(TemplateTreeSyncPath, false);
		}


		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			//check if a templateId is assigned, meaning we are editing a template
			if (!Request.QueryString["templateID"].IsNullOrWhiteSpace())
			{
				_template = new Template(int.Parse(Request.QueryString["templateID"]));
                TemplateTreeSyncPath = "-1,init," + _template.Path.Replace("-1,", "");
			}
			else if (!Request.QueryString["file"].IsNullOrWhiteSpace())
			{
				//we are editing a view (i.e. partial view)
				OriginalFileName = HttpUtility.UrlDecode(Request.QueryString["file"]);

                //TemplateTreeSyncPath = "-1,init," + Path.GetFileName(OriginalFileName);

                TemplateTreeSyncPath = DeepLink.GetTreePathFromFilePath(OriginalFileName.TrimStart("MacroPartials/").TrimStart("Partials/"));
			}
			else
			{
				throw new InvalidOperationException("Cannot render the editor without a supplied templateId or a file");
			}
			
			Panel1.hasMenu = true;
            var editor = Panel1.NewTabPage(ui.Text("template"));
            editor.Controls.Add(Pane8);

            var props = Panel1.NewTabPage(ui.Text("properties"));
            props.Controls.Add(Pane7);


            SaveButton = Panel1.Menu.NewButton();
            SaveButton.Text = ui.Text("save");
            SaveButton.ButtonType = MenuButtonType.Primary;
            SaveButton.ID = "save";
            SaveButton.CssClass = "client-side";

			Panel1.Text = ui.Text("edittemplate");
			pp_name.Text = ui.Text("name", base.getUser());
			pp_alias.Text = ui.Text("alias", base.getUser());
			pp_masterTemplate.Text = ui.Text("mastertemplate", base.getUser());

			// Editing buttons
            MenuIconI umbField = editorSource.Menu.NewIcon();
			umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
			umbField.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
					editorSource.ClientID + "&tagName=UMBRACOGETDATA&mvcView=true", ui.Text("template", "insertPageField"), 640, 550);
			umbField.AltText = ui.Text("template", "insertPageField");


			// TODO: Update icon
            MenuIconI umbDictionary = editorSource.Menu.NewIcon();
			umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
			umbDictionary.OnClickCommand =
				ClientTools.Scripts.OpenModalWindow(
					IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
                    editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY&mvcView=true", ui.Text("template", "insertDictionaryItem"),
					640, 550);
			umbDictionary.AltText = "Insert umbraco dictionary item";

			var macroSplitButton = new InsertMacroSplitButton
				{
					ClientCallbackInsertMacroMarkup = "function(alias) {editViewEditor.insertMacroMarkup(alias);}",
					ClientCallbackOpenMacroModel = "function(alias) {editViewEditor.openMacroModal(alias);}"
				};
            editorSource.Menu.InsertNewControl(macroSplitButton, 40);

            MenuIconI umbTemplateQueryBuilder = editorSource.Menu.NewIcon();
            umbTemplateQueryBuilder.ImageURL = UmbracoPath + "/images/editor/inshtml.gif";
            umbTemplateQueryBuilder.OnClickCommand = "editViewEditor.openQueryModal()";
            umbTemplateQueryBuilder.AltText = "Open query builder";

			if (_template == null)
			{
				InitializeEditorForPartialView();
			}
			else
			{
				InitializeEditorForTemplate();	
			}
			
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
			ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
		}
		
		/// <summary>
		/// Configure the editor for partial view editing
		/// </summary>
		private void InitializeEditorForPartialView()
		{
			pp_masterTemplate.Visible = false;
			pp_alias.Visible = false;
			pp_name.Text = "Filename";
		}

		/// <summary>
		/// Configure the editor for editing a template
		/// </summary>
		private void InitializeEditorForTemplate()
		{

            //TODO: implement content placeholders, etc... just like we had in v5

            editorSource.Menu.InsertSplitter();

            MenuIconI umbRenderBody = editorSource.Menu.NewIcon();
            umbRenderBody.ImageURL = UmbracoPath + "/images/editor/renderbody.gif";
            //umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
            umbRenderBody.AltText = "Insert @RenderBody()";

            umbRenderBody.OnClickCommand = "editViewEditor.insertRenderBody()";

            MenuIconI umbSection = editorSource.Menu.NewIcon();
            umbSection.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
            //umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
            umbSection.AltText = "Insert Section";

            umbSection.OnClickCommand = "editViewEditor.openSnippetModal('section')";

            MenuIconI umbRenderSection = editorSource.Menu.NewIcon();
            umbRenderSection.ImageURL = UmbracoPath + "/images/editor/masterpageContent.gif";
            //umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
            umbRenderSection.AltText = "Insert @RenderSection";

            umbRenderSection.OnClickCommand = "editViewEditor.openSnippetModal('rendersection')";

        }

    }
}