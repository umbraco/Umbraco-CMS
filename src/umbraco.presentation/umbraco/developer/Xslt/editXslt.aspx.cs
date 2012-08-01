using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using umbraco.BasePages;
using umbraco.uicontrols;
using System.Net;
using umbraco.cms.presentation.Trees;
using umbraco.IO;
using umbraco.cms.helpers;

namespace umbraco.cms.presentation.developer
{
    /// <summary>
    /// Summary description for editXslt.
    /// </summary>
    public partial class editXslt : UmbracoEnsuredPage
    {
        public editXslt()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }
        protected PlaceHolder buttons;



        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string file = Request.QueryString["file"];
                string path = DeepLink.GetTreePathFromFilePath(file);
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadXslt>().Tree.Alias)
                    .SyncTree(path, false);
            }



        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);


            uicontrols.MenuIconI save = UmbracoPanel1.Menu.NewIcon();
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save Xslt File";
            save.ID = "save";

            UmbracoPanel1.Menu.InsertSplitter();

            uicontrols.MenuIconI tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/images/editor/insField.GIF";
            tmp.OnClickCommand = ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/developer/xslt/xsltinsertvalueof.aspx?objectId=" + editorSource.ClientID, "Insert value", 750, 250);
            //"umbracoInsertField(document.getElementById('editorSource'), 'xsltInsertValueOf', '','felt', 750, 230, '');";
            tmp.AltText = "Insert xslt:value-of";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = SystemDirectories.Umbraco + "/images/editor/insMemberItem.GIF";
            tmp.OnClickCommand = "UmbEditor.Insert('<xsl:variable name=\"\" select=\"', '\"/>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:variable";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = SystemDirectories.Umbraco + "/images/editor/insChildTemplateNew.GIF";
            tmp.OnClickCommand = "UmbEditor.Insert('<xsl:if test=\"CONDITION\">\\n', '\\n</xsl:if>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:if";

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = SystemDirectories.Umbraco + "/images/editor/insChildTemplateNew.GIF";
            tmp.OnClickCommand = "UmbEditor.Insert('<xsl:for-each select=\"QUERY\">\\n', '\\n</xsl:for-each>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:for-each";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = SystemDirectories.Umbraco + "/images/editor/insFieldByLevel.GIF";
            tmp.OnClickCommand = "UmbEditor.Insert('<xsl:choose>\\n<xsl:when test=\"CONDITION\">\\n', '\\n</xsl:when>\\n<xsl:otherwise>\\n</xsl:otherwise>\\n</xsl:choose>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:choose";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = SystemDirectories.Umbraco + "/images/editor/xslVisualize.GIF";
            tmp.OnClickCommand = "xsltVisualize();";
            tmp.AltText = "Visualize XSLT";


            // Add source and filename
            String file = IOHelper.MapPath(SystemDirectories.Xslt + "/" + Request.QueryString["file"]);

            // validate file
            IOHelper.ValidateEditPath(file, SystemDirectories.Xslt);
            // validate extension
            IOHelper.ValidateFileExtension(file, new List<string>() { "xslt", "xsl" });


            xsltFileName.Text = file.Replace(IOHelper.MapPath(SystemDirectories.Xslt), "").Substring(1);

            StreamReader SR;
            string S;
            SR = File.OpenText(file);

            S = SR.ReadToEnd();
            SR.Close();

            editorSource.Text = S;
        }


        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.Webservices) + "/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.Webservices) + "/legacyAjaxCalls.asmx"));
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
