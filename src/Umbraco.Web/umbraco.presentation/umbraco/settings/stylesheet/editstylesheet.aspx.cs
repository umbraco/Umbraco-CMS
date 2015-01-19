using System;
using System.Linq;
using System.Web.UI;
using Umbraco.Core.IO;
using Umbraco.Web;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.uicontrols;
using Umbraco.Core;

namespace umbraco.cms.presentation.settings.stylesheet
{
    /// <summary>
    /// Summary description for editstylesheet.
    /// </summary>
    public partial class editstylesheet : UmbracoEnsuredPage
    {
        private Umbraco.Core.Models.Stylesheet _sheet;

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

            _sheet = Services.FileService.GetStylesheetByName(Request.QueryString["id"]);
            if (_sheet == null) throw new InvalidOperationException("No stylesheet found with name: " + Request.QueryString["id"]);

            lttPath.Text = "<a target='_blank' href='" + _sheet.VirtualPath + "'>" + _sheet.VirtualPath + "</a>";


            if (IsPostBack == false)
            {
                NameTxt.Text = _sheet.Path.TrimEnd(".css");
                editorSource.Text = _sheet.Content;

                ClientTools
                    .SetActiveTreeType(Constants.Trees.Stylesheets)
                    .SyncTree("-1,init," + _sheet.Path
                        //needs a double escape to work with JS
                        .Replace("\\", "\\\\").TrimEnd(".css"), false);
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