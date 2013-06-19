using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Xsl;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.uicontrols;
using System.Net;
using umbraco.cms.presentation.Trees;
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

        protected MenuIconI SaveButton;

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

		protected override void OnInit(EventArgs e)
		{			
			base.OnInit(e);
            
            SaveButton = UmbracoPanel1.Menu.NewIcon();
            SaveButton.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            SaveButton.AltText = "Save Xslt File";
            SaveButton.ID = "save";

			UmbracoPanel1.Menu.InsertSplitter();

			var tmp = UmbracoPanel1.Menu.NewIcon();
			tmp.ImageURL = IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/images/editor/insField.GIF";
			tmp.OnClickCommand = ClientTools.Scripts.OpenModalWindow(IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/developer/xslt/xsltinsertvalueof.aspx?objectId=" + editorSource.ClientID, "Insert value", 750, 250);
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
			var file = IOHelper.MapPath(SystemDirectories.Xslt + "/" + Request.QueryString["file"]);

			// validate file
			IOHelper.ValidateEditPath(file, SystemDirectories.Xslt);
			// validate extension
			IOHelper.ValidateFileExtension(file, new List<string>() { "xslt", "xsl" });


			xsltFileName.Text = file.Replace(IOHelper.MapPath(SystemDirectories.Xslt), "").Substring(1).Replace(@"\", "/");

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

			ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices) + "/codeEditorSave.asmx"));
			ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices) + "/legacyAjaxCalls.asmx"));
		}


		/// <summary>
		/// JsInclude1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

		/// <summary>
		/// UmbracoPanel1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.UmbracoPanel UmbracoPanel1;

		/// <summary>
		/// Pane1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane1;

		/// <summary>
		/// pp_filename control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_filename;

		/// <summary>
		/// xsltFileName control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox xsltFileName;

		/// <summary>
		/// pp_testing control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_testing;

		/// <summary>
		/// SkipTesting control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.CheckBox SkipTesting;

		/// <summary>
		/// pp_errorMsg control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.PropertyPanel pp_errorMsg;

		/// <summary>
		/// editorSource control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.CodeArea editorSource;

		/// <summary>
		/// editorJs control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Literal editorJs;
	}
}
