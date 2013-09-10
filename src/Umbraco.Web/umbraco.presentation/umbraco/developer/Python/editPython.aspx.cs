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
using System.Xml;
using System.Xml.Xsl;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.presentation.Trees;
using umbraco.cms.helpers;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.developer
{
    public partial class editPython : BasePages.UmbracoEnsuredPage
    {
        public editPython()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }

        protected MenuButton SaveButton;

        private readonly List<string> _allowedExtensions = new List<string>();
        protected PlaceHolder buttons;
        protected CodeArea CodeArea1;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UmbracoPanel1.hasMenu = true;

            if (!IsPostBack)
            {
                string file = Request.QueryString["file"];
                string path = DeepLink.GetTreePathFromFilePath(file);
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadPython>().Tree.Alias)
                    .SyncTree(path, false);
            }
        }

        private List<string> validScriptingExtensions()
        {
            if (_allowedExtensions.Count == 0)
            {
                foreach (MacroEngineLanguage lang in MacroEngineFactory.GetSupportedUILanguages())
                {
                    if (!_allowedExtensions.Contains(lang.Extension))
                        _allowedExtensions.Add(lang.Extension);
                }
            }

            return _allowedExtensions;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SaveButton = UmbracoPanel1.Menu.NewButton();
           
            SaveButton.ToolTip = "Save scripting File";
            SaveButton.Text = ui.Text("save");
            SaveButton.OnClientClick = "return false;";
            SaveButton.ID = "save";
            SaveButton.ButtonType = MenuButtonType.Primary;


            var code = UmbracoPanel1.NewTabPage("code");
            code.Controls.Add(Pane1);

            var props = UmbracoPanel1.NewTabPage("properties");
            props.Controls.Add(Pane2);


            // Add source and filename
            String file = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + Request.QueryString["file"]);

            // validate file
            IOHelper.ValidateEditPath(file, SystemDirectories.MacroScripts);
            // validate extension
            IOHelper.ValidateFileExtension(file, validScriptingExtensions());

            pythonFileName.Text = file.Replace(IOHelper.MapPath(SystemDirectories.MacroScripts), "").Substring(1).Replace(@"\", "/");

            StreamReader SR;
            string S;
            SR = File.OpenText(file);
            S = SR.ReadToEnd();
            SR.Close();
            pythonSource.Text = S;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices) + "/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference(IOHelper.ResolveUrl(SystemDirectories.WebServices) + "/legacyAjaxCalls.asmx"));
        }

        /// <summary>
        /// UmbracoPanel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.TabView UmbracoPanel1;

        /// <summary>
        /// Pane1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane1;
        protected global::umbraco.uicontrols.Pane Pane2;

        /// <summary>
        /// pp_filename control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_filename;

        /// <summary>
        /// pythonFileName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox pythonFileName;

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
        /// pythonSource control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.CodeArea pythonSource;
    }
}
