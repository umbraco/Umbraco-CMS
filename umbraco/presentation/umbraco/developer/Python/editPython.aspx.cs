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
using System.Xml;
using System.Xml.Xsl;
using System.Text.RegularExpressions;
using umbraco.cms.presentation.Trees;
using umbraco.IO;

namespace umbraco.cms.presentation.developer
{
    public partial class editPython : BasePages.UmbracoEnsuredPage
    {

        protected PlaceHolder buttons;
        protected uicontrols.CodeArea CodeArea1;

        private void Page_Load(object sender, System.EventArgs e)
        {
            UmbracoPanel1.hasMenu = true;

			if (!IsPostBack)
			{
				ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadPython>().Tree.Alias)
					.SyncTree(Request.QueryString["file"], false);
			}
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            uicontrols.MenuIconI save = UmbracoPanel1.Menu.NewIcon();
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save scripting File";

            // Add source and filename
            String file = Request.QueryString["file"];
            pythonFileName.Text = file;

            StreamReader SR;
            string S;
            SR = File.OpenText( IOHelper.MapPath(SystemDirectories.Python + "/" + file));
            S = SR.ReadToEnd();
            SR.Close();
            pythonSource.Text = S;
        }

        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.Webservices) + "/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.Webservices) + "/legacyAjaxCalls.asmx"));
        }
    }
}
