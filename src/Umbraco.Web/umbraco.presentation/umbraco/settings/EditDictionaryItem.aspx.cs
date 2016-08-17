using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.presentation.Trees;
using Umbraco.Core;
using Umbraco.Core.Models;
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

		protected void Page_Load(object sender, System.EventArgs e)
		{
		    currentItem = Services.LocalizationService.GetDictionaryItemById(int.Parse(Request.QueryString["id"]));                

			// Put user code to initialize the page here
			Panel1.hasMenu = true;
			Panel1.Text = Services.TextService.Localize("editdictionary") + ": " + currentItem.ItemKey;
			
            uicontrols.Pane p = new uicontrols.Pane();

			var save = Panel1.Menu.NewButton();
            save.Text = Services.TextService.Localize("save");
            save.Click += save_Click;
			save.ToolTip = Services.TextService.Localize("save");
            save.ID = "save";
            save.ButtonType = uicontrols.MenuButtonType.Primary;

            Literal txt = new Literal();
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

                    Services.LocalizationService.Save(currentItem);

                }
			}
            ClientTools.ShowSpeechBubble(SpeechBubbleIcon.Save, Services.TextService.Localize("speechBubbles/dictionaryItemSaved"), "");	
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
	}
}
