using System;
using System.Web.UI;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation.Trees;
using umbraco.IO;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.settings.stylesheet
{
    /// <summary>
    /// Summary description for editstylesheet.
    /// </summary>
    public partial class editstylesheet : UmbracoEnsuredPage
    {
        private StyleSheet stylesheet;

        public editstylesheet()
        {
            CurrentApp = DefaultApps.settings.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // editor source

                if (!UmbracoSettings.ScriptDisableEditor)
                {
                    // TODO: Register the some script editor js file if you can find a good one.
                }

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadStylesheets>().Tree.Alias)
                    .SyncTree("-1,init," + helper.Request("id"), false);
            }

            MenuIconI save = Panel1.Menu.NewIcon();
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save stylesheet";
            save.ID = "save";
            Panel1.Text = ui.Text("stylesheet", "editstylesheet", base.getUser());
            pp_name.Text = ui.Text("name", base.getUser());
            pp_path.Text = ui.Text("path", base.getUser());

            stylesheet = new StyleSheet(int.Parse(Request.QueryString["id"]));
            string appPath = Request.ApplicationPath;
            if (appPath == "/")
                appPath = "";
            lttPath.Text = "<a target='_blank' href='" + appPath + "/css/" + stylesheet.Text + ".css'>" + appPath +
                            IO.SystemDirectories.Css + "/" + stylesheet.Text + ".css</a>";


            if (!IsPostBack)
            {
                NameTxt.Text = stylesheet.Text;
                editorSource.Text = stylesheet.Content;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //

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

        #endregion
    }
}