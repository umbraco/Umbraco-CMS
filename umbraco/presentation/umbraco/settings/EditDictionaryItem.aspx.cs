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
using umbraco.cms.presentation.Trees;
using umbraco.IO;

namespace umbraco.settings
{
	/// <summary>
	/// Summary description for EditDictionaryItem.
	/// </summary>
	public partial class EditDictionaryItem : BasePages.UmbracoEnsuredPage
	{
	    public EditDictionaryItem()
	    {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();

	    }
		protected LiteralControl keyTxt = new LiteralControl();
		protected uicontrols.TabView tbv = new uicontrols.TabView();
		private System.Collections.ArrayList languageFields = new System.Collections.ArrayList();
        private cms.businesslogic.Dictionary.DictionaryItem currentItem;

		protected void Page_Load(object sender, System.EventArgs e)
		{
			currentItem = new cms.businesslogic.Dictionary.DictionaryItem(int.Parse(Request.QueryString["id"]));

			// Put user code to initialize the page here
			Panel1.hasMenu = true;
			Panel1.Text = ui.Text("editdictionary") + ": " + currentItem.key;
			
            uicontrols.Pane p = new uicontrols.Pane();

			ImageButton save = Panel1.Menu.NewImageButton();
			save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
			save.AlternateText = ui.Text("save");
            save.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";

            Literal txt = new Literal();
            txt.Text = "<p>" + ui.Text("dictionaryItem", "description", currentItem.key, base.getUser()) + "</p><br/>";
            p.addProperty(txt);
			
			foreach (cms.businesslogic.language.Language l in cms.businesslogic.language.Language.getAll)
			{
                /*
				uicontrols.TabPage tp = tbv.NewTabPage(l.CultureAlias);
				tp.HasMenu = false;
				languageTextbox tmp = new languageTextbox(l.id);

				if (!IsPostBack) 
					tmp.Text = currentItem.Value(l.id);

				languageFields.Add(tmp);
				tp.Controls.Add(tmp);
                 */

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
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadDictionary>().Tree.Alias)
					.SyncTree(helper.Request("id"), false);
			}


            Panel1.Controls.Add(p);
		}

		private void save_click(object sender, System.Web.UI.ImageClickEventArgs e) {
			
            foreach (TextBox t in languageFields) {
				if (t.Text != "") {
					currentItem.setValue(int.Parse(t.ID),t.Text);
				}
			}
            speechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "dictionaryItemSaved"),"");	
		}
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
            /*
			tbv.ID="tabview1";
			tbv.Width = 400;
			tbv.Height = 200;
		*/

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
				set {_languageid = value;}
				get {return _languageid;}
			}
			public languageTextbox(int languageId) : base() {
				this.TextMode = TextBoxMode.MultiLine;
				this.Rows = 10;
				this.Columns = 40;
				this.Attributes.Add("style", "margin: 3px; width: 98%;");
		
				this.languageid = languageId;
			}
		}
	}
}
