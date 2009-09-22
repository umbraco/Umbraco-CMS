using System;
using System.Collections;
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

namespace umbraco.cms.presentation.settings.scripts
{
    public partial class editScript : BasePages.UmbracoEnsuredPage
    {
        protected System.Web.UI.HtmlControls.HtmlForm Form1;
        protected uicontrols.UmbracoPanel Panel1;
        protected System.Web.UI.WebControls.TextBox NameTxt;
        protected uicontrols.Pane Pane7;

        protected System.Web.UI.WebControls.Literal lttPath;
        protected System.Web.UI.WebControls.Literal editorJs;
        protected umbraco.uicontrols.CodeArea editorSource;
        protected umbraco.uicontrols.PropertyPanel pp_name;
        protected umbraco.uicontrols.PropertyPanel pp_path;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            String file = Request.QueryString["file"];
            NameTxt.Text = file;

            //need to change the editor type if it is XML
            if (file.EndsWith("xml"))
                editorSource.CodeBase = umbraco.uicontrols.CodeArea.EditorType.XML;

            string appPath = Request.ApplicationPath;
            if (appPath == "/")
                appPath = "";

            lttPath.Text = "<a target='_blank' href='" + appPath + UmbracoSettings.ScriptFolderPath + "/" + Request.QueryString["file"] + "'>" + appPath + UmbracoSettings.ScriptFolderPath + "/" + Request.QueryString["file"] + "</a>";
			            
            string openPath = Server.MapPath(UmbracoSettings.ScriptFolderPath + "/" + file);

            //security check... only allow
            if (openPath.StartsWith(Server.MapPath(UmbracoSettings.ScriptFolderPath + "/")))
            {
                StreamReader SR;
                string S;
                SR = File.OpenText(Server.MapPath(UmbracoSettings.ScriptFolderPath + "/" + file));
                S = SR.ReadToEnd();
                SR.Close();
                
                editorSource.Text = S;
            }
            
            Panel1.Text = ui.Text("editscript", base.getUser());
            pp_name.Text = ui.Text("name", base.getUser());
            pp_path.Text = ui.Text("path", base.getUser());

			if (!IsPostBack)
			{
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadScripts>().Tree.Alias)
					.SyncTree(file, false);
			}
        }

       

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            uicontrols.MenuIconI save = Panel1.Menu.NewIcon();
            save.ImageURL = GlobalSettings.Path + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save File";
            

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



          protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }
        #endregion

    }
}
