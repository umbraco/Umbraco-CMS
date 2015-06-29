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
using Umbraco.Web;
using umbraco.cms.presentation.Trees;

namespace umbraco.cms.presentation.settings.stylesheet
{
    /// <summary>
    /// Summary description for EditStyleSheetProperty.
    /// </summary>
    public partial class EditStyleSheetProperty : BasePages.UmbracoEnsuredPage
    {
        public EditStyleSheetProperty()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

        }

        private businesslogic.web.StylesheetProperty _stylesheetproperty;
        private DropDownList _ddl = new DropDownList();

        protected void Page_Load(object sender, EventArgs e)
        {
            _stylesheetproperty = new businesslogic.web.StylesheetProperty(int.Parse(Request.QueryString["id"]));
            Panel1.Text = ui.Text("stylesheet", "editstylesheetproperty", UmbracoUser);

            if (IsPostBack == false)
            {
                _stylesheetproperty.RefreshFromFile();
                NameTxt.Text = _stylesheetproperty.Text;
                Content.Text = _stylesheetproperty.value;
                AliasTxt.Text = _stylesheetproperty.Alias;

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadStylesheetProperty>().Tree.Alias)
                    .SyncTree(Request.GetItemAsString("id"), false);
            }
            else
            {
                //true = force reload from server on post back
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadStylesheetProperty>().Tree.Alias)
                    .SyncTree(Request.GetItemAsString("id"), true);
            }

            

            var bt = Panel1.Menu.NewButton();
            bt.Click += SaveClick;
            bt.Text = ui.Text("save");
            bt.ToolTip = ui.Text("save");
            bt.ButtonType = uicontrols.MenuButtonType.Primary;
            bt.ID = "save";
            SetupPreView();
        }

        protected override void OnPreRender(EventArgs e)
        {
            prStyles.Attributes["style"] = _stylesheetproperty.value;

            base.OnPreRender(e);
        }

        private void SetupPreView()
        {
            prStyles.Attributes["style"] = _stylesheetproperty.value;
        }

        private void SaveClick(object sender, EventArgs e)
        {
            _stylesheetproperty.value = Content.Text;
            _stylesheetproperty.Text = NameTxt.Text;
            _stylesheetproperty.Alias = AliasTxt.Text;

            try
            {
                _stylesheetproperty.StyleSheet().saveCssToFile();
            }
            catch { }
            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editStylesheetPropertySaved", UmbracoUser), "");
            SetupPreView();

            _stylesheetproperty.Save();
        }


        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Content.TextMode = TextBoxMode.MultiLine;
            Content.Height = 250;
            Content.Width = 300;
        }


        /// <summary>
        /// Panel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.UmbracoPanel Panel1;

        /// <summary>
        /// Pane7 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane Pane7;

        /// <summary>
        /// NameTxt control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox NameTxt;

        /// <summary>
        /// AliasTxt control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox AliasTxt;

        /// <summary>
        /// Content control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox Content;

        /// <summary>
        /// prStyles control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl prStyles;
    }
}
