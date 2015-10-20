using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.cms.presentation.Trees;
using System.Linq;
using umbraco.cms.helpers;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.settings.scripts
{
    public partial class editScript : BasePages.UmbracoEnsuredPage
    {
        public editScript()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }
        protected System.Web.UI.HtmlControls.HtmlForm Form1;
        protected uicontrols.TabView Panel1;
        protected System.Web.UI.WebControls.TextBox NameTxt;
        protected uicontrols.Pane Pane7;
        protected uicontrols.Pane Pane8;

        protected System.Web.UI.WebControls.Literal lttPath;
        protected System.Web.UI.WebControls.Literal editorJs;
        protected umbraco.uicontrols.CodeArea editorSource;
        protected umbraco.uicontrols.PropertyPanel pp_name;
        protected umbraco.uicontrols.PropertyPanel pp_path;

        protected MenuButton SaveButton;

        private string filename;
        protected string ScriptTreeSyncPath { get; private set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // get the script, ensure it exists (not null) and validate (because
            // the file service ensures that it loads scripts from the proper location
            // but does not seem to validate extensions?) - in case of an error,
            // throw - that's what we did anyways.

            // also scrapping the code that added .cshtml and .vbhtml extensions, and
            // ~/Views directory - we're not using editScript.aspx for views anymore.

            var svce = ApplicationContext.Current.Services.FileService;
            var script = svce.GetScriptByName(filename);
            if (script == null) // not found
                throw new FileNotFoundException("Could not find file '" + filename + "'.");

            lttPath.Text = "<a id=\"" + lttPath.ClientID + "\" target=\"_blank\" href=\"" + script.VirtualPath + "\">" + script.VirtualPath + "</a>";
            editorSource.Text = script.Content;
            ScriptTreeSyncPath = DeepLink.GetTreePathFromFilePath(filename);

            // name derives from filename, clean for xss
            NameTxt.Text = filename.CleanForXss('\\', '/');

            Panel1.Text = ui.Text("editscript", base.getUser());
            pp_name.Text = ui.Text("name", base.getUser());
            pp_path.Text = ui.Text("path", base.getUser());

            if (IsPostBack == false)
            {
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadScripts>().Tree.Alias)
                    .SyncTree(ScriptTreeSyncPath, false);
            }
        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            filename = Request.QueryString["file"].Replace('\\', '/').TrimStart('/');

            //need to change the editor type if it is XML
            if (filename.EndsWith("xml"))
                editorSource.CodeBase = uicontrols.CodeArea.EditorType.XML;
            else if (filename.EndsWith("master"))
                editorSource.CodeBase = uicontrols.CodeArea.EditorType.HTML;


            var editor = Panel1.NewTabPage(ui.Text("settings","script"));
            editor.Controls.Add(Pane7);

            var props = Panel1.NewTabPage(ui.Text("properties"));
            props.Controls.Add(Pane8);


            SaveButton = Panel1.Menu.NewButton();
            SaveButton.Text = ui.Text("save");
            SaveButton.ButtonType = MenuButtonType.Primary;
            SaveButton.ID = "save";
            SaveButton.CssClass = "client-side";

            if (editorSource.CodeBase == uicontrols.CodeArea.EditorType.HTML)
            {
                // Editing buttons
                Panel1.Menu.InsertSplitter();
                uicontrols.MenuIconI umbField = Panel1.Menu.NewIcon();
                umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
                umbField.OnClickCommand = BasePages.ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDATA", ui.Text("template", "insertPageField"), 640, 550);
                umbField.AltText = ui.Text("template", "insertPageField");

                // TODO: Update icon
                uicontrols.MenuIconI umbDictionary = Panel1.Menu.NewIcon();
                umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
                umbDictionary.OnClickCommand = BasePages.ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" + editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY", ui.Text("template", "insertDictionaryItem"), 640, 550);
                umbDictionary.AltText = "Insert umbraco dictionary item";

                uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
                umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
                umbMacro.AltText = ui.Text("template", "insertMacro");
                umbMacro.OnClickCommand = BasePages.ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/editMacro.aspx?objectId=" + editorSource.ClientID, ui.Text("template", "insertMacro"), 470, 530);

                // Help
                Panel1.Menu.InsertSplitter();

                uicontrols.MenuIconI helpIcon = Panel1.Menu.NewIcon();
                helpIcon.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(Umbraco.Core.IO.IOHelper.ResolveUrl(Umbraco.Core.IO.SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=", ui.Text("template", "quickGuide"), 600, 580);
                helpIcon.ImageURL = UmbracoPath + "/images/editor/help.png";
                helpIcon.AltText = ui.Text("template", "quickGuide");

            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

    }
}
