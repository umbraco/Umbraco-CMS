using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.UI;

namespace umbraco.settings
{
    /// <summary>
    /// Summary description for EditDictionaryItem.
    /// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Dictionary)]
	public partial class EditDictionaryItem : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        protected LiteralControl keyTxt = new LiteralControl();
        protected uicontrols.TabView tbv = new uicontrols.TabView();
        private System.Collections.ArrayList languageFields = new System.Collections.ArrayList();
        private IDictionaryItem currentItem;
        protected TextBox boxChangeKey;
        protected Label labelChangeKey;
        protected Literal txt;

        protected void Page_Load(object sender, System.EventArgs e)
        {
		    currentItem = Services.LocalizationService.GetDictionaryItemById(int.Parse(Request.QueryString["id"]));

            // Put user code to initialize the page here
            Panel1.hasMenu = true;
			Panel1.Text = Services.TextService.Localize("editdictionary") + ": " + currentItem.ItemKey;

            var save = Panel1.Menu.NewButton();
            save.Text = Services.TextService.Localize("save");
            save.Click += save_Click;
			save.ToolTip = Services.TextService.Localize("save");
            save.ID = "save";
            save.ButtonType = uicontrols.MenuButtonType.Primary;

            uicontrols.Pane p = new uicontrols.Pane();

            boxChangeKey = new TextBox
            {
                ID = "changeKey-" + currentItem.Id,
                CssClass = "umbEditorTextField",
                Text = currentItem.ItemKey
            };

            labelChangeKey = new Label
            {
                ID = "changeKeyLabel",
                CssClass = "text-error"
            };

            p.addProperty(new Literal
            {
                Text = "<p>" + Services.TextService.Localize("dictionaryItem/changeKey") + "</p>"
            });
            p.addProperty(boxChangeKey);
            p.addProperty(labelChangeKey);


            txt = new Literal();
            txt.Text = "<p>" + Services.TextService.Localize("dictionaryItem/description", new[] { currentItem.ItemKey }) + "</p><br/>";
            p.addProperty(txt);

            foreach (cms.businesslogic.language.Language l in cms.businesslogic.language.Language.getAll)
            {

                TextBox languageBox = new TextBox();
                languageBox.TextMode = TextBoxMode.MultiLine;
                languageBox.ID = l.id.ToString();
                languageBox.CssClass = "umbEditorTextFieldMultiple";

			    if (!IsPostBack)
			    {
			        languageBox.Text = currentItem.GetTranslatedValue(l.id);
			    }

                languageFields.Add(languageBox);
                p.addProperty(l.FriendlyName, languageBox);

            }


            if (!IsPostBack)
            {
                var path = BuildPath(currentItem);
                ClientTools
					.SetActiveTreeType(Constants.Trees.Dictionary)
                    .SyncTree(path, false);
            }

            Panel1.Controls.Add(p);
        }

	    private string BuildPath(IDictionaryItem current)
        {
	        var parentPath = current.ParentId.HasValue == false ? "" : BuildPath(current) + ",";
	        return parentPath + current.Id;
        }

        void save_Click(object sender, EventArgs e)
        {
            labelChangeKey.Text = ""; // reset error text
            var newKey = boxChangeKey.Text;
            var save = true;
            if (string.IsNullOrWhiteSpace(newKey) == false && newKey != currentItem.ItemKey)
            {
                if (Services.LocalizationService.DictionaryItemExists(newKey))
                {
                    // reject
                    labelChangeKey.Text = Services.TextService.Localize("dictionaryItem/changeKeyError", newKey);
                    boxChangeKey.Text = currentItem.ItemKey; // reset key
                    save = false;
                }
                else
                {
                    // update key
                    currentItem.ItemKey = newKey;

                    // update title
                    Panel1.title.InnerHtml = Services.TextService.Localize("editdictionary") + ": " + newKey;

                    // sync the content tree
                    var path = BuildPath(currentItem);
                    ClientTools.SyncTree(path, true);
                }
            }

            if (save)
            {
                foreach (TextBox t in languageFields)
                {
                    //check for null but allow empty string!
                    // http://issues.umbraco.org/issue/U4-1931
                    if (t.Text != null)
                    {
                        Services.LocalizationService.AddOrUpdateDictionaryValue(
                            currentItem,
                            Services.LocalizationService.GetLanguageById(int.Parse(t.ID)),
                            t.Text);
                    }
                }

                Services.LocalizationService.Save(currentItem);
                ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, Services.TextService.Localize("speechBubbles/dictionaryItemSaved"), "");
            }

            txt.Text = "<br/><p>" + Services.TextService.Localize("dictionaryItem/description", currentItem.ItemKey) + "</p><br/>";
        }

        #region Web Form Designer generated code

        override protected void OnInit(EventArgs e)
        {
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
