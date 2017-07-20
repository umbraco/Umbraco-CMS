using System;
using System.Collections;
using System.Globalization;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.UI;

namespace umbraco.settings
{
    /// <summary>
    /// Summary description for editLanguage.
    /// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Languages)]
    public partial class editLanguage : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        public editLanguage()
        {
            CurrentApp = Constants.Applications.Settings.ToString();

        }
        protected System.Web.UI.WebControls.TextBox NameTxt;
        protected System.Web.UI.WebControls.Literal DisplayName;
        //cms.businesslogic.language.Language currentLanguage;
        private ILanguage lang;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            //currentLanguage = new cms.businesslogic.language.Language(int.Parse(Request.GetItemAsString("id")));
            lang = Current.Services.LocalizationService.GetLanguageById(int.Parse(Request.GetItemAsString("id")));


            // Put user code to initialize the page here
            Panel1.Text = Services.TextService.Localize("editlanguage");
            pp_language.Text = Services.TextService.Localize("language/displayName");
            if (!IsPostBack)
            {
                updateCultureList();

                ClientTools
                    .SetActiveTreeType(Constants.Trees.Languages)
                    .SyncTree(Request.GetItemAsString("id"), false);
            }

        }

        private void updateCultureList()
        {
            SortedList sortedCultures = new SortedList();
            Cultures.Items.Clear();
            Cultures.Items.Add(new ListItem(Services.TextService.Localize("choose") + "...", ""));
            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                sortedCultures.Add(ci.DisplayName + "|||" + Guid.NewGuid().ToString(), ci.Name);

            IDictionaryEnumerator ide = sortedCultures.GetEnumerator();
            while (ide.MoveNext())
            {
                ListItem li = new ListItem(ide.Key.ToString().Substring(0, ide.Key.ToString().IndexOf("|||")), ide.Value.ToString());
                if (ide.Value.ToString() == lang.IsoCode)
                    li.Selected = true;

                Cultures.Items.Add(li);
            }
        }

        private void save_click(object sender, EventArgs e)
        {
            //currentLanguage.CultureAlias = Cultures.SelectedValue;
            //currentLanguage.Save();
            lang.IsoCode = Cultures.SelectedValue;
            Current.Services.LocalizationService.Save(lang);
            updateCultureList();

            ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, Services.TextService.Localize("speechBubbles/languageSaved"), "");
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            Panel1.hasMenu = true;
            var save = Panel1.Menu.NewButton();
            save.Click += save_click;
            save.ToolTip = Services.TextService.Localize("save");
            save.Text = Services.TextService.Localize("save");
            save.ID = "save";
            save.ButtonType = Umbraco.Web._Legacy.Controls.MenuButtonType.Primary;

            Panel1.Text = Services.TextService.Localize("language/editLanguage");

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
