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
using umbraco.cms.businesslogic.macro;
using umbraco.cms.presentation.Trees;
using umbraco.IO;

namespace umbraco.cms.presentation.developer
{
    public partial class editPython : BasePages.UmbracoEnsuredPage
    {
        public editPython()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }

        private List<string> allowedExtensions = new List<string>();
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

        private List<string> validScriptingExtensions()
        {
            if (allowedExtensions.Count == 0)
            {
                foreach (MacroEngineLanguage lang in MacroEngineFactory.GetSupportedUILanguages())
                {
                    if (!allowedExtensions.Contains(lang.Extension))
                        allowedExtensions.Add(lang.Extension);
                }
            }

            return allowedExtensions;
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            uicontrols.MenuIconI save = UmbracoPanel1.Menu.NewIcon();
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = "Save scripting File";
            save.ID = "save";

            // Add source and filename
            String file = IOHelper.MapPath(SystemDirectories.MacroScripts + "/" + Request.QueryString["file"]);

            // validate file
            IOHelper.ValidateEditPath(file, SystemDirectories.MacroScripts);
            // validate extension
            IOHelper.ValidateFileExtension(file, validScriptingExtensions());

            // we need to move the full path and then the preceeding slash
            pythonFileName.Text = file.Replace(IOHelper.MapPath(SystemDirectories.MacroScripts), "").Substring(1);

            StreamReader SR;
            string S;
            SR = File.OpenText(file);
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
