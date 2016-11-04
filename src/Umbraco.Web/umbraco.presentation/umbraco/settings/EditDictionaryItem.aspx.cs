using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;

namespace umbraco.settings
{
    /// <summary>
    /// Summary description for EditDictionaryItem.
    /// </summary>
    [WebformsPageTreeAuthorize(Constants.Trees.Dictionary)]
    public partial class EditDictionaryItem : BasePages.UmbracoEnsuredPage
    {

        protected LiteralControl keyTxt = new LiteralControl();
        protected uicontrols.TabView tbv = new uicontrols.TabView();
        private System.Collections.ArrayList languageFields = new System.Collections.ArrayList();
        private cms.businesslogic.Dictionary.DictionaryItem currentItem;
        protected TextBox boxChangeKey;
        protected Label labelChangeKey;
        protected Literal txt;
        protected User currentUser;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            currentItem = new cms.businesslogic.Dictionary.DictionaryItem(int.Parse(Request.QueryString["id"]));
            currentUser = getUser();

            // Put user code to initialize the page here
            Panel1.hasMenu = true;
            Panel1.Text = ui.Text("editdictionary") + ": " + currentItem.key;

            var save = Panel1.Menu.NewButton();
            save.Text = ui.Text("save");
            save.Click += save_Click;
            save.ToolTip = ui.Text("save");
            save.ID = "save";
            save.ButtonType = uicontrols.MenuButtonType.Primary;

            uicontrols.Pane p = new uicontrols.Pane();

            boxChangeKey = new TextBox
            {
                ID = "changeKey-" + currentItem.id,
                CssClass = "umbEditorTextField",
                Text = currentItem.key
            };

            labelChangeKey = new Label
            {
                ID = "changeKeyLabel",
                CssClass = "text-error"
            };

            p.addProperty(new Literal
            {
                Text = "<p>" + ui.Text("dictionaryItem", "changeKey", currentUser) + "</p>"
            });
            p.addProperty(boxChangeKey);
            p.addProperty(labelChangeKey);


            txt = new Literal();
            txt.Text = "<br/><p>" + ui.Text("dictionaryItem", "description", currentItem.key, currentUser) + "</p><br/>";
            p.addProperty(txt);

            foreach (cms.businesslogic.language.Language l in cms.businesslogic.language.Language.getAll)
            {

                TextBox languageBox = new TextBox();
                languageBox.TextMode = TextBoxMode.MultiLine;
                languageBox.ID = l.id.ToString();
                languageBox.CssClass = "umbEditorTextFieldMultiple";

                if (!IsPostBack)
                    languageBox.Text = currentItem.Value(l.id);

                languageFields.Add(languageBox);
                p.addProperty(l.FriendlyName, languageBox);

            }


            if (!IsPostBack)
            {
                var path = BuildPath(currentItem);
                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadDictionary>().Tree.Alias)
                    .SyncTree(path, false);
            }

            Panel1.Controls.Add(p);
        }

        private string BuildPath(cms.businesslogic.Dictionary.DictionaryItem current)
        {
            var parentPath = current.IsTopMostItem() ? "" : BuildPath(current.Parent) + ",";
            return parentPath + current.id;
        }

        void save_Click(object sender, EventArgs e)
        {
            foreach (TextBox t in languageFields)
            {
                //check for null but allow empty string!
                // http://issues.umbraco.org/issue/U4-1931
                if (t.Text != null)
                {
                    currentItem.setValue(int.Parse(t.ID), t.Text);
                }
            }

            labelChangeKey.Text = ""; // reset error text 
            var newKey = boxChangeKey.Text;
            if (string.IsNullOrWhiteSpace(newKey) == false && newKey != currentItem.key)
            {
                // key already exists, save but inform
                if (Dictionary.DictionaryItem.hasKey(newKey) == true)
                {
                    labelChangeKey.Text = ui.Text("dictionaryItem", "changeKeyError", newKey, currentUser);
                    boxChangeKey.Text = currentItem.key; // reset key                    
                }
                else
                {
                    // set the new key
                    currentItem.setKey(newKey);

                    // update the title with the new key
                    Panel1.title.InnerHtml = ui.Text("editdictionary") + ": " + newKey;

                    // sync the content tree
                    var path = BuildPath(currentItem);
                    ClientTools.SyncTree(path, true);
                }
            }
            txt.Text = "<br/><p>" + ui.Text("dictionaryItem", "description", currentItem.key, currentUser) + "</p><br/>";
            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "dictionaryItemSaved"), "");
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


        private class languageTextbox : TextBox
        {

            private int _languageid;

            public int languageid
            {
                set { _languageid = value; }
                get { return _languageid; }
            }

            public languageTextbox(int languageId) : base()
            {
                this.TextMode = TextBoxMode.MultiLine;
                this.Rows = 10;
                this.Columns = 40;
                this.Attributes.Add("style", "margin: 3px; width: 98%;");

                this.languageid = languageId;
            }
        }
    }
}
