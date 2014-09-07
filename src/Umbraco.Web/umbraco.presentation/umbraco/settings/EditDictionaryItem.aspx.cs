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
using Umbraco.Core;
using Umbraco.Core.IO;

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

		protected void Page_Load(object sender, System.EventArgs e)
		{
			currentItem = new cms.businesslogic.Dictionary.DictionaryItem(int.Parse(Request.QueryString["id"]));

			// Put user code to initialize the page here
			Panel1.hasMenu = true;
			Panel1.Text = ui.Text("editdictionary") + ": " + currentItem.key;
			
            uicontrols.Pane p = new uicontrols.Pane();

			var save = Panel1.Menu.NewButton();
            save.Text = ui.Text("save");
            save.Click += save_Click;
			save.ToolTip = ui.Text("save");
            save.ID = "save";
            save.ButtonType = uicontrols.MenuButtonType.Primary;

            Literal txt = new Literal();
            txt.Text = "<p>" + ui.Text("dictionaryItem", "description", currentItem.key, base.getUser()) + "</p><br/>";
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
					currentItem.setValue(int.Parse(t.ID),t.Text);
				}
			}
            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "dictionaryItemSaved"), "");	
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
