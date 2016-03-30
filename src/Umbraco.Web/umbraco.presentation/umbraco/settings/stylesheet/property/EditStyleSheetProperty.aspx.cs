using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Services.Description;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Web;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Web.UI;
using Umbraco.Core.Services;

namespace umbraco.cms.presentation.settings.stylesheet
{
    /// <summary>
    /// Summary description for EditStyleSheetProperty.
    /// </summary>
    public partial class EditStyleSheetProperty : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        public EditStyleSheetProperty()
        {
            CurrentApp = Constants.Applications.Settings.ToString();

        }

        private Umbraco.Core.Models.StylesheetProperty _stylesheetproperty;
        private Umbraco.Core.Models.Stylesheet _sheet;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            _sheet = Services.FileService.GetStylesheetByName(Request.QueryString["id"]);
            if (_sheet == null) throw new InvalidOperationException("No stylesheet found with name: " + Request.QueryString["id"]);

            var propName = IsPostBack ? OriginalName.Value : Request.QueryString["prop"];

            _stylesheetproperty = _sheet.Properties.FirstOrDefault(x => x.Name.InvariantEquals(propName));
            if (_stylesheetproperty == null) throw new InvalidOperationException("No stylesheet property found with name: " + Request.QueryString["prop"]);

            Panel1.Text = Services.TextService.Localize("stylesheet/editstylesheetproperty");

            var bt = Panel1.Menu.NewButton();
            bt.Click += SaveClick;
            bt.Text = Services.TextService.Localize("save");
            bt.ToolTip = Services.TextService.Localize("save");
            bt.ButtonType = uicontrols.MenuButtonType.Primary;
            bt.ID = "save";
        }


        protected override void OnPreRender(EventArgs e)
        {
            NameTxt.Text = _stylesheetproperty.Name;
            Content.Text = _stylesheetproperty.Value;
            AliasTxt.Text = _stylesheetproperty.Alias;
            OriginalName.Value = _stylesheetproperty.Name;

            prStyles.Attributes["style"] = _stylesheetproperty.Value;

            var path = _sheet.Path.Replace('\\', '/');
            var nodePath = string.Format(BaseTree.GetTreePathFromFilePath(path) +
                        ",{0}_{1}", path, _stylesheetproperty.Name);

            ClientTools
                    .SetActiveTreeType(Constants.Trees.Stylesheets)
                    .SyncTree(nodePath, IsPostBack);

            prStyles.Attributes["style"] = _stylesheetproperty.Value;

            base.OnPreRender(e);
        }

        private void SaveClick(object sender, EventArgs e)
        {
            _stylesheetproperty.Value = Content.Text;
            _stylesheetproperty.Alias = AliasTxt.Text;

            if (_stylesheetproperty.Name != NameTxt.Text)
            {
                //to change the name we actually have to remove the property and re-add it as a different one
                _sheet.AddProperty(new Umbraco.Core.Models.StylesheetProperty(NameTxt.Text, _stylesheetproperty.Alias, _stylesheetproperty.Value));
                _sheet.RemoveProperty(_stylesheetproperty.Name);
                //reset our variable 
                _stylesheetproperty = _sheet.Properties.Single(x => x.Name == NameTxt.Text);
            }

            Services.FileService.SaveStylesheet(_sheet);

            ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, Services.TextService.Localize("speechBubbles/editStylesheetPropertySaved"), "");
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

        protected global::System.Web.UI.WebControls.HiddenField OriginalName;

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
