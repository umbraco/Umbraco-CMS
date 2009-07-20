using System;
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

namespace umbraco.cms.presentation.developer
{
    /// <summary>
    /// Summary description for editXslt.
    /// </summary>
    public partial class editXslt : UmbracoEnsuredPage
    {
        protected PlaceHolder buttons;
  


        protected void Page_Load(object sender, EventArgs e)
        {
			if (!IsPostBack)
			{
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadXslt>().Tree.Alias)
					.SyncTree(Request.QueryString["file"], false);
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
            save.ImageURL = GlobalSettings.Path + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save Xslt File";


            UmbracoPanel1.Menu.InsertSplitter();
           
            uicontrols.MenuIconI tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/insField.GIF";
            tmp.OnClickCommand = "top.openModal('developer/xslt/xsltinsertvalueof.aspx?objectId=" + editorSource.ClientID + "', 'Insert value', 250, 750);";
                //"umbracoInsertField(document.getElementById('editorSource'), 'xsltInsertValueOf', '','felt', 750, 230, '');";
            tmp.AltText = "Insert xslt:value-of";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/insMemberItem.GIF";
            tmp.OnClickCommand = "insertCode('<xsl:variable name=\"\" select=\"', '\"/>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:variable";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/insChildTemplateNew.GIF";
            tmp.OnClickCommand = "insertCode('<xsl:if test=\"CONDITION\">\\n', '\\n</xsl:if>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:if";

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/insChildTemplateNew.GIF";
            tmp.OnClickCommand = "insertCode('<xsl:for-each select=\"QUERY\">\\n', '\\n</xsl:for-each>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:for-each";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/insFieldByLevel.GIF";
            tmp.OnClickCommand = "insertCode('<xsl:choose>\\n<xsl:when test=\"CONDITION\">\\n', '\\n</xsl:when>\\n<xsl:otherwise>\\n</xsl:otherwise>\\n</xsl:choose>\\n', '" + editorSource.ClientID + "'); return false;";
            tmp.AltText = "Insert xsl:choose";

            UmbracoPanel1.Menu.InsertSplitter();

            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/xslVisualize.GIF";
            tmp.OnClickCommand = "xsltVisualize();";
            tmp.AltText = "Visualize XSLT";

            /*
            tmp = UmbracoPanel1.Menu.NewIcon();
            tmp.ImageURL = GlobalSettings.Path + "/images/editor/insFieldByTree.GIF";
            tmp.OnClickCommand = "insertCodeAtCaret(document.getElementById('" + editorSource.ClientID + "'), '<xsl:template name=\"\">\\n\\t<xsl:param name=\"\"/>\\n</xsl:template>\\n');";
            tmp.AltText = "Insert xsl:template with match";
            */

            // Add source and filename
            String file = Request.QueryString["file"];

            //Hardcoded Fix/Hack, form can only open and edit xslt files.. PPH
            if (file.ToLower().EndsWith(".xslt"))
            {
                xsltFileName.Text = file;

                StreamReader SR;
                string S;
                SR = File.OpenText(Server.MapPath(GlobalSettings.Path + "/../xslt/" + file));

                S = SR.ReadToEnd();
                SR.Close();

                editorSource.Text = S;
                //editorSource.Attributes.Add("onKeyDown", "AllowTabCharacter();");
            }
        }


        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
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