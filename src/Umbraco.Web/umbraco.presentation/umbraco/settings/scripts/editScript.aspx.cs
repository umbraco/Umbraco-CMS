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
using umbraco.cms.presentation.Trees;
using umbraco.IO;
using System.Linq;
using umbraco.cms.helpers;

namespace umbraco.cms.presentation.settings.scripts
{
    public partial class editScript : BasePages.UmbracoEnsuredPage
    {
        public editScript()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }
        protected System.Web.UI.HtmlControls.HtmlForm Form1;
        protected uicontrols.UmbracoPanel Panel1;
        protected System.Web.UI.WebControls.TextBox NameTxt;
        protected uicontrols.Pane Pane7;

        protected System.Web.UI.WebControls.Literal lttPath;
        protected System.Web.UI.WebControls.Literal editorJs;
        protected umbraco.uicontrols.CodeArea editorSource;
        protected umbraco.uicontrols.PropertyPanel pp_name;
        protected umbraco.uicontrols.PropertyPanel pp_path;

        private string file;

        protected void Page_Load(object sender, System.EventArgs e)
        {

            NameTxt.Text = file;

            string path = "";
            if (file.StartsWith("~/"))
                path = IOHelper.ResolveUrl(file);
            else
                path = IOHelper.ResolveUrl(SystemDirectories.Scripts + "/" + file);


            lttPath.Text = "<a target='_blank' href='" + path + "'>" + path + "</a>";

            var exts = UmbracoSettings.ScriptFileTypes.Split(',').ToList();
            if (UmbracoSettings.EnableMvcSupport)
            {
                exts.Add("cshtml");
                exts.Add("vbhtml");
            }

            var dirs = Umbraco.Core.IO.SystemDirectories.Scripts;
            if (UmbracoSettings.EnableMvcSupport)
                dirs += "," + Umbraco.Core.IO.SystemDirectories.MvcViews;

            // validate file
            IOHelper.ValidateEditPath(IOHelper.MapPath(path), dirs.Split(','));
            
            // validate extension
            IOHelper.ValidateFileExtension(IOHelper.MapPath(path), exts);


            StreamReader SR;
            string S;
            SR = File.OpenText(IOHelper.MapPath(path));
            S = SR.ReadToEnd();
            SR.Close();

            editorSource.Text = S;

            Panel1.Text = ui.Text("editscript", base.getUser());
            pp_name.Text = ui.Text("name", base.getUser());
            pp_path.Text = ui.Text("path", base.getUser());

            if (!IsPostBack)
            {
                string sPath = DeepLink.GetTreePathFromFilePath(file);
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadScripts>().Tree.Alias)
                    .SyncTree(sPath, false);
            }
        }



        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {

            file = Request.QueryString["file"].TrimStart('/');

            //need to change the editor type if it is XML
            if (file.EndsWith("xml"))
                editorSource.CodeBase = umbraco.uicontrols.CodeArea.EditorType.XML;
            else if (file.EndsWith("master"))
                editorSource.CodeBase = umbraco.uicontrols.CodeArea.EditorType.HTML;

            uicontrols.MenuIconI save = Panel1.Menu.NewIcon();
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save File";
            save.ID = "save";

            if (editorSource.CodeBase == uicontrols.CodeArea.EditorType.HTML)
            {
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

                // Help
                Panel1.Menu.InsertSplitter();

                uicontrols.MenuIconI helpIcon = Panel1.Menu.NewIcon();
                helpIcon.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=", ui.Text("template", "quickGuide"), 600, 580);
                helpIcon.ImageURL = UmbracoPath + "/images/editor/help.png";
                helpIcon.AltText = ui.Text("template", "quickGuide");

            }


            this.Load += new System.EventHandler(Page_Load);
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



        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }
        #endregion

    }
}
