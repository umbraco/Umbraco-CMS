using System;
using System.Web.UI;
using Umbraco.Core.IO;
using Umbraco.Web;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.settings.stylesheet
{
    /// <summary>
    /// Summary description for editstylesheet.
    /// </summary>
    public partial class editstylesheet : UmbracoEnsuredPage
    {
        private StyleSheet stylesheet;

        protected MenuButton SaveButton;

        public editstylesheet()
        {
            CurrentApp = DefaultApps.settings.ToString();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);


            var editor = Panel1.NewTabPage(ui.Text("stylesheet"));
            editor.Controls.Add(Pane7);

            var props = Panel1.NewTabPage(ui.Text("properties"));
            props.Controls.Add(Pane8);


            SaveButton = Panel1.Menu.NewButton();
            SaveButton.Text = ui.Text("save");
            SaveButton.ButtonType = MenuButtonType.Primary;
            SaveButton.ID = "save";
            SaveButton.CssClass = "client-side";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            Panel1.Text = ui.Text("stylesheet", "editstylesheet", UmbracoUser);
            pp_name.Text = ui.Text("name", UmbracoUser);
            pp_path.Text = ui.Text("path", UmbracoUser);

            stylesheet = new StyleSheet(int.Parse(Request.QueryString["id"]));
            var appPath = Request.ApplicationPath;
            if (appPath == "/")
                appPath = "";
            lttPath.Text = "<a target='_blank' href='" + appPath + "/css/" + stylesheet.Text + ".css'>" + appPath +
                            SystemDirectories.Css + "/" + stylesheet.Text + ".css</a>";


            if (IsPostBack == false)
            {
                NameTxt.Text = stylesheet.Text;
                editorSource.Text = stylesheet.Content;

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadStylesheets>().Tree.Alias)
                    .SyncTree("-1,init," + Request.GetItemAsString("id"), false);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

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
        /// pp_path control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_path;

        /// <summary>
        /// lttPath control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal lttPath;

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

    }
}